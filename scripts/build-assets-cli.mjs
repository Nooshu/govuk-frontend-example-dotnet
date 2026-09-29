import { buildAssets } from './build-assets.mjs';

const manifest = await buildAssets();
process.stdout.write(`assets ${manifest.stylesheet} ${manifest.script} ${manifest.assetPrefix}\n`);
