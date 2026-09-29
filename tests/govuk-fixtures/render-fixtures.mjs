import nunjucks from 'nunjucks';
import { readdir, readFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../..');

/**
 * Render every stored fixture through Nunjucks.
 * Leading and trailing whitespace is trimmed because that is how the published
 * fixture HTML relates to the macro output. This check only proves the fixture
 * files still match the pinned Frontend package.
 */
export async function nunjucksFixtureMismatches() {
  const env = nunjucks.configure(path.join(root, 'node_modules/govuk-frontend/dist'), {
    autoescape: true,
    trimBlocks: true,
    lstripBlocks: true,
  });
  const components = path.join(root, 'node_modules/govuk-frontend/dist/govuk/components');
  const mismatches = [];
  for (const name of await readdir(components)) {
    const file = path.join(components, name, 'fixtures.json');
    let data;
    try {
      data = JSON.parse(await readFile(file, 'utf8'));
    } catch {
      continue;
    }
    for (const fixture of data.fixtures) {
      const html = env
        .render(`govuk/components/${name}/template.njk`, { params: fixture.options })
        .trim();
      if (html !== fixture.html) {
        mismatches.push(`${name} / ${fixture.name}`);
      }
    }
  }
  return mismatches;
}
