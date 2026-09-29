import { createHash } from 'node:crypto';
import { mkdir, readFile, readdir, writeFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

import { buildStyles } from './build-styles.mjs';

const ROOT = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');

/**
 * Fingerprint compiled CSS, the initAll module, and GOV.UK fonts and images.
 * Fonts and images share one content hash so the Sass asset path can point at them.
 * CSS and JavaScript get their own hashes because their bytes change independently.
 * @param {{ root?: string, outDir?: string }} [options]
 */
export async function buildAssets(options = {}) {
  if (options === null || typeof options !== 'object' || Array.isArray(options)) {
    throw new TypeError('options must be an object');
  }

  const root = options.root ?? ROOT;
  const outDir = options.outDir ?? path.join(root, 'src/GovUk.Frontend.Example/wwwroot');
  const frontend = path.join(root, 'node_modules/govuk-frontend/dist/govuk');
  const { css } = await buildStyles({
    entry: path.join(root, 'styles/application.scss'),
    outFile: path.join(root, 'dist/stylesheets/application.css'),
  });

  const staticFiles = await collectFiles(path.join(frontend, 'assets'));
  const staticHash = hashFiles(staticFiles);
  const assetPrefix = `/assets/${staticHash}/`;
  const fingerprintedCss = css.replaceAll('url(/assets/', `url(${assetPrefix}`);
  const cssHash = sha(fingerprintedCss);

  const frontendJs = await readFile(path.join(frontend, 'govuk-frontend.min.js'), 'utf8');
  const appJs = `import { initAll } from './govuk-frontend.min.js';\n\ninitAll();\n`;
  const jsHash = sha(frontendJs + '\n' + appJs);

  await writeTree(
    path.join(outDir, 'assets', staticHash),
    staticFiles.map((file) => ({
      relative: file.relative,
      contents: file.contents,
    })),
  );
  await writeFile(path.join(outDir, 'assets', `${cssHash}.css`), fingerprintedCss);
  const jsDir = path.join(outDir, 'assets', jsHash);
  await mkdir(jsDir, { recursive: true });
  await writeFile(path.join(jsDir, 'govuk-frontend.min.js'), frontendJs);
  await writeFile(path.join(jsDir, 'app.mjs'), appJs);

  const manifest = {
    stylesheet: `/assets/${cssHash}.css`,
    script: `/assets/${jsHash}/app.mjs`,
    assetPrefix,
  };
  await mkdir(outDir, { recursive: true });
  await writeFile(
    path.join(outDir, 'asset-manifest.json'),
    `${JSON.stringify(manifest, null, 2)}\n`,
  );
  return manifest;
}

async function collectFiles(directory) {
  const files = [];
  async function walk(current) {
    for (const entry of await readdir(current, { withFileTypes: true })) {
      const full = path.join(current, entry.name);
      if (entry.isDirectory()) {
        await walk(full);
        continue;
      }
      files.push({
        relative: path.relative(directory, full).split(path.sep).join('/'),
        contents: await readFile(full),
      });
    }
  }
  await walk(directory);
  files.sort((left, right) => left.relative.localeCompare(right.relative));
  return files;
}

function hashFiles(files) {
  const hash = createHash('sha256');
  for (const file of files) {
    hash.update(file.relative);
    hash.update(file.contents);
  }
  return hash.digest('hex').slice(0, 10);
}

function sha(value) {
  return createHash('sha256').update(value).digest('hex').slice(0, 10);
}

async function writeTree(directory, files) {
  for (const file of files) {
    const destination = path.join(directory, file.relative);
    await mkdir(path.dirname(destination), { recursive: true });
    await writeFile(destination, file.contents);
  }
}
