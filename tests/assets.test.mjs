import assert from 'node:assert/strict';
import { mkdtemp, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import path from 'node:path';
import test from 'node:test';

import { buildAssets } from '../scripts/build-assets.mjs';

test('fingerprints css, javascript, and static assets', async () => {
  const outDir = await mkdtemp(path.join(tmpdir(), 'govuk-assets-'));
  try {
    const manifest = await buildAssets({ outDir });
    assert.match(manifest.stylesheet, /^\/assets\/[a-f0-9]{10}\.css$/);
    assert.match(manifest.script, /^\/assets\/[a-f0-9]{10}\/app\.mjs$/);
    assert.match(manifest.assetPrefix, /^\/assets\/[a-f0-9]{10}\/$/);
    const again = await buildAssets({ outDir });
    assert.deepEqual(again, manifest);
  } finally {
    await rm(outDir, { recursive: true, force: true });
  }
});

test('writes the default asset manifest', async () => {
  const manifest = await buildAssets();
  assert.match(manifest.stylesheet, /^\/assets\/[a-f0-9]{10}\.css$/);
});

test('accepts an explicit repository root', async () => {
  const outDir = await mkdtemp(path.join(tmpdir(), 'govuk-assets-root-'));
  try {
    const manifest = await buildAssets({
      root: path.join(import.meta.dirname, '..'),
      outDir,
    });
    assert.match(manifest.stylesheet, /^\/assets\/[a-f0-9]{10}\.css$/);
  } finally {
    await rm(outDir, { recursive: true, force: true });
  }
});

test('rejects invalid asset build options', async () => {
  await assert.rejects(() => buildAssets(null), /options must be an object/);
  await assert.rejects(() => buildAssets([]), /options must be an object/);
});
