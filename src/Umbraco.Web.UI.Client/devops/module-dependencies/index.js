import fs from 'fs';
import path from 'path';
import { createImportMap } from '../importmap/index.js';

const modulePrefix = '@umbraco-cms/backoffice/';

// Regex patterns to match import and require statements
const importRegex = /import\s+(?:[^'"]+\s+from\s+)?['"]([^'"]+)['"]/g;

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

	/*
	if (imports.length > 0) {
		console.log(`\n📁 File: ${filePath}`);
		imports.forEach((imp) => {
			console.log(`  → ${imp.type.toUpperCase()}: ${imp.value}`);
		});
	}
	*/

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

function getFolderPathFromModuleName(moduleName) {
	const importMap = createImportMap({
		rootDir: './src',
	});
	const importMapEntries = Object.entries(importMap.imports);
	const importMapEntry = importMapEntries.find(([key]) => key === modulePrefix + moduleName);
	if (!importMapEntry) {
		throw new Error(`Module not found: ${moduleName}`);
	}

	// remove everything after the last /
	const lastSlashIndex = importMapEntry[1].lastIndexOf('/');
	const modulePath = importMapEntry[1].substring(0, lastSlashIndex);

	return modulePath;
}

// Change the path below to the folder you want to scan
if (!process.env.MODULE_NAME) {
	throw new Error('Please set the MODULE_NAME environment variable to the alias you want to scan.');
}

const modulePath = getFolderPathFromModuleName(process.env.MODULE_NAME);
const clientProjectRoot = path.resolve(import.meta.dirname, '../../');
const targetFolder = path.resolve(clientProjectRoot, modulePath);
const imports = getAllImportsFromFolder(targetFolder);
const importValues = imports.map((imp) => imp.value);
const uniqueImports = [...new Set(importValues)];
const umbracoModuleImports = uniqueImports
	.filter((imp) => imp.startsWith(modulePrefix))
	.sort((a, b) => a.localeCompare(b));

console.log(`\n\n📦 Imports found in ${modulePrefix + process.env.MODULE_NAME}:\n`);
console.log(umbracoModuleImports);
