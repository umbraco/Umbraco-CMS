import { cpSync, rmSync } from 'fs';
import { execSync } from 'child_process';

const srcDir = './dist-cms';
const outputDir = '../Umbraco.Cms.StaticAssets/wwwroot/umbraco/backoffice';

// Copy assets
console.log('--- Copying assets ---');
cpSync('./src/assets', `${srcDir}/assets`, { recursive: true });
console.log('--- Copying assets done ---');

// Copy SRC CSS
console.log('--- Copying src CSS ---');
cpSync('./src/css', `${srcDir}/css`, { recursive: true });
console.log('--- Copying src CSS done ---');

// Minify CSS
console.log('--- Minifying CSS ---');
execSync('npx postcss dist-cms/css/**/*.css --replace --use cssnano --verbose', { stdio: 'inherit' });
console.log('--- Minifying CSS done ---');

rmSync(outputDir, { recursive: true, force: true });
cpSync(srcDir, outputDir, { recursive: true });

console.log('--- Copied build output to CMS successfully. ---');
