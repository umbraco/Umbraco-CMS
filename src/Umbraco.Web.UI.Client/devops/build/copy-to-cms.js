import { cpSync } from 'fs';

const srcDir = './dist-cms';
const outputDir = '../Umbraco.Cms.StaticAssets/wwwroot/umbraco/backoffice';
const outputWebDir = '../Umbraco.Web.UI.New';

cpSync(srcDir, outputDir, { recursive: true });
cpSync(`${srcDir}/umbraco-package-schema.json`, `${outputWebDir}/umbraco-package-schema.json`);
