import fs from 'fs';
import path from 'path';
import { createImportMap } from '../importmap/index.js';

const clientProjectRoot = path.resolve(import.meta.dirname, '../../');
const modulePrefix = '@umbraco-cms/backoffice/';

// Regex patterns to match import and require statements
const importRegex = /import\s+(?:[^'"]+\s+from\s+)?['"]([^'"]+)['"]/g;

const importMap = createImportMap({
	rootDir: './src',
});

const importMapEntries = Object.entries(importMap.imports);
const coreModules = importMapEntries.filter(([key, value]) => value.includes('/packages/core/'));
const coreModuleAliases = coreModules.map(([key]) => key);

const packageModules = importMapEntries.filter(
	([key, value]) => value.includes('/packages/') && !value.includes('/packages/core/'),
);
const packageModuleAliases = packageModules.map(([key]) => key);

/**
 * Recursively walk through a directory and return all file paths
 */
function getAllFiles(dirPath, arrayOfFiles = []) {
	const files = fs.readdirSync(dirPath);

	files.forEach((file) => {
		const fullPath = path.join(dirPath, file);
		if (fs.statSync(fullPath).isDirectory()) {
			getAllFiles(fullPath, arrayOfFiles);
		} else {
			arrayOfFiles.push(fullPath);
		}
	});

	return arrayOfFiles;
}

/**
 * Scan a file and extract import statements
 */
function getImportsInFile(filePath) {
	const ext = path.extname(filePath);
	if (filePath.includes('.stories.ts')) return;
	if (filePath.includes('.test.ts')) return;
	if (!['.ts'].includes(ext)) return;

	const content = fs.readFileSync(filePath, 'utf-8');
	const imports = [];

	let match;
	while ((match = importRegex.exec(content)) !== null) {
		imports.push({ type: 'import', value: match[1] });
	}

	return imports;
}

// Entry point
function getAllImportsFromFolder(startPath) {
	if (!fs.existsSync(startPath)) {
		console.error(`Path does not exist: ${startPath}`);
		return;
	}

	const files = getAllFiles(startPath);

	const imports = files
		.map((file) => {
			return getImportsInFile(file);
		})
		.flat()
		.filter(Boolean);

	return imports;
}

function getFolderPathFromModuleAlias(moduleAlias) {
	const importMapEntry = importMapEntries.find(([key]) => key === moduleAlias);
	if (!importMapEntry) {
		throw new Error(`Module not found: ${moduleAlias}`);
	}

	// remove everything after the last /
	const lastSlashIndex = importMapEntry[1].lastIndexOf('/');
	const modulePath = importMapEntry[1].substring(0, lastSlashIndex);

	return modulePath;
}

function getUmbracoModuleImportsInModule(moduleAlias) {
	const modulePath = getFolderPathFromModuleAlias(moduleAlias);
	const targetFolder = path.resolve(clientProjectRoot, modulePath);
	const imports = getAllImportsFromFolder(targetFolder);
	const importValues = imports.map((imp) => imp.value);
	const uniqueImports = [...new Set(importValues)];
	const umbracoModuleImports = uniqueImports
		.filter((imp) => imp.startsWith(modulePrefix))
		.sort((a, b) => a.localeCompare(b));
	return umbracoModuleImports;
}

function reportIllegalImportsFromCore() {
	let numberOfModulesWithIllegalImports = 0;
	// Check if any of the core modules import one of the package modules
	// Run through all core modules and find the imports
	coreModules.forEach(([alias, path]) => {
		const modules = getUmbracoModuleImportsInModule(alias);
		const illegalImports = modules.filter((imp) => packageModuleAliases.includes(imp));
		if (illegalImports.length === 0) return;
		console.error(`🚨 ${alias}: Illegal imports found:`);
		illegalImports.forEach((imp) => {
			console.error(`  → ${imp}`);
		});
		console.log(`\n`);
		numberOfModulesWithIllegalImports++;
	});

	if (numberOfModulesWithIllegalImports === 0) {
		console.log(`✅ No illegal imports found in core modules.`);
	} else {
		throw new Error(`Illegal imports found in ${numberOfModulesWithIllegalImports} core modules.`);
	}
}

// Change the path below to the folder you want to scan
if (!process.env.MODULE_NAME) {
	throw new Error('Please set the MODULE_NAME environment variable to the alias you want to scan.');
}

reportIllegalImportsFromCore();
