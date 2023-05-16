// Load all .d.ts files from the dist/libs folder
// and replace all imports from @umbraco-cms/backoffice with relative imports
// Example: import { Foo } from '@umbraco-cms/backoffice/element-api' -> import { Foo } from './element'
// This is needed because the d.ts files are not in the same folder as the source files
// and the absolute paths are not valid when the d.ts files are copied to the dist folder
// This is only used when building the d.ts files.
//
// This script also copies the package.json and README.md files to the dist/libs folder
// and the umbraco-package-schema.json file to the Umbraco.Web.UI.New folder
//
// Usage: node utils/move-libs.js

import { readdirSync, readFileSync, writeFileSync, cpSync, mkdirSync } from 'fs';

const srcDir = './libs';
const inputDir = './dist/libs';
const outputDir = '../Umbraco.Cms.StaticAssets/wwwroot/umbraco/backoffice/libs';
const executableDir = '../Umbraco.Web.UI.New';

// Copy package files
cpSync(`${srcDir}/package.json`, `${inputDir}/package.json`, { recursive: true });
console.log(`Copied ${srcDir}/package.json to ${inputDir}/package.json`);
cpSync(`${srcDir}/README.md`, `${inputDir}/README.md`, { recursive: true });
console.log(`Copied ${srcDir}/README.md to ${inputDir}/README.md`);
cpSync(`${inputDir}/umbraco-package-schema.json`, `${executableDir}/umbraco-package-schema.json`, { recursive: true });
console.log(`Copied ${inputDir}/umbraco-package-schema.json to ${executableDir}/umbraco-package-schema.json`);

const libs = readdirSync(inputDir);

// Create output folder
try {
	mkdirSync(outputDir, { recursive: true });
} catch {
	// Ignore
}

// Transform all .d.ts files and copy all other files to the output folder
libs.forEach((lib) => {
	if (lib.endsWith('.js') === false && lib.endsWith('.js.map') === false) return;

	console.log(`Transforming ${lib}`);

	const dtsFile = `${inputDir}/${lib}`;

	let code = readFileSync(dtsFile, 'utf8');

	// Replace all absolute imports with relative imports
	if (lib.endsWith('.d.ts')) {
		code = code.replace(/from '(@umbraco-cms\/backoffice\/[^']+)'/g, (match, p1) => {
			return `from './${p1.split('/').pop()}'`;
		});
	}

	writeFileSync(dtsFile, code, 'utf8');

	cpSync(dtsFile, `${outputDir}/${lib}`, { recursive: true });
});
