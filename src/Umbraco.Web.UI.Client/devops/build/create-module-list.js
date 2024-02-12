import { createImportMap } from "../importmap/index.js";
import { writeFileSync, rmSync } from "fs";

const srcDir = './dist-cms';
const outputModuleList = `${srcDir}/.backoffice-modules`;
const importMap = createImportMap({ rootDir: '', additionalImports: {} });

// Create a list of modules to be copied to the CMS
let moduleList = '';

for (const [src, dest] of Object.entries(importMap.imports)) {
	if (src === '.' || !dest || src.includes('examples')) continue;
	moduleList += `${src} ${dest}\n`;
}

rmSync(outputModuleList, { force: true });
writeFileSync(outputModuleList, moduleList);
console.log(`Wrote module list to ${outputModuleList}`);
