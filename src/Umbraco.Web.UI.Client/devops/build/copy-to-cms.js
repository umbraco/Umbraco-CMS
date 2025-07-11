import { cpSync, rmSync } from 'fs';

const srcDir = './dist-cms';
const outputDir = '../Umbraco.Cms.StaticAssets/wwwroot/umbraco/backoffice';

rmSync(outputDir, { recursive: true, force: true });
cpSync(srcDir, outputDir, { recursive: true });

// Copy assets
console.log('--- Copying assets ---');
cpSync('./src/assets', `${outputDir}/assets`, { recursive: true });
console.log('--- Copying assets done ---');

// Copy SRC CSS
console.log('--- Copying src CSS ---');
cpSync('./src/css', `${outputDir}/css`, { recursive: true });
console.log('--- Copying src CSS done ---');

console.log('--- Copied build output to CMS successfully. ---');
