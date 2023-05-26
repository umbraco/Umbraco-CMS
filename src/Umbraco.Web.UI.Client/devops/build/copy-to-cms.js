import { cpSync } from 'fs';

// TODO: Simplified version of utils/move-libs.js
// We need to update this to support the same parts as move-libs.js
const srcDir = './dist-cms';
const outputDir = '../Umbraco.Cms.StaticAssets/wwwroot/umbraco/backoffice';

cpSync(srcDir, outputDir, { recursive: true });
