import fs from 'fs';
import path from 'path';

// Regex patterns to match import and require statements
const importRegex = /import\s+(?:[^'"]+\s+from\s+)?['"]([^'"]+)['"]/g;
const requireRegex = /require\(\s*['"]([^'"]+)['"]\s*\)/g;

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

// Change the path below to the folder you want to scan
const targetFolder = path.resolve(import.meta.dirname, '../../src/packages/ufm');
const imports = getAllImportsFromFolder(targetFolder);
const importValues = imports.map((imp) => imp.value);
const uniqueImports = [...new Set(importValues)];
const umbracoModuleImports = uniqueImports.filter((imp) => imp.startsWith('@umbraco'));
console.log(umbracoModuleImports);
