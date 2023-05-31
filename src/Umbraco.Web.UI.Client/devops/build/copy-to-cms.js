import { cpSync } from 'fs';

const srcDir = './dist-cms';
const outputDir = '../Umbraco.Cms.StaticAssets/wwwroot/umbraco/backoffice';

cpSync(srcDir, outputDir, { recursive: true });
