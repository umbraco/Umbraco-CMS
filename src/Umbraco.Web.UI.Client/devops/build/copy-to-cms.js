import { cpSync, rmSync } from 'fs';

const srcDir = './dist-cms';
const outputDir = '../Umbraco.Cms.StaticAssets/wwwroot/umbraco/backoffice';

rmSync(outputDir, { recursive: true });
cpSync(srcDir, outputDir, { recursive: true });
