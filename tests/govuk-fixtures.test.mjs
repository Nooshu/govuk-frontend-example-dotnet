import assert from 'node:assert/strict';
import test from 'node:test';

import { nunjucksFixtureMismatches } from './govuk-fixtures/render-fixtures.mjs';

test('stored fixtures still match the pinned Nunjucks macros', async () => {
  const mismatches = await nunjucksFixtureMismatches();
  assert.deepEqual(mismatches, []);
});
