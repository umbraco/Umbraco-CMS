import { createImportMap } from "../importmap/index.js";
import { writeFileSync, rmSync } from "fs";
import { packageJsonName, packageJsonVersion } from "../package/index.js";

const srcDir = './dist-cms';
const outputModuleList = `${srcDir}/umbraco-package.json`;
const importmap = createImportMap({ rootDir: '/umbraco/backoffice', additionalImports: {} });

const umbracoPackageJson = {
	name: packageJsonName,
	version: packageJsonVersion,
	extensions: [],
	importmap
};

try {
	rmSync(outputModuleList, { force: true });
	writeFileSync(outputModuleList, JSON.stringify(umbracoPackageJson));
	console.log(`Wrote manifest to ${outputModuleList}`);
} catch (e) {
	console.error(`Failed to write manifest to ${outputModuleList}`, e);
	process.exit(1);
}
