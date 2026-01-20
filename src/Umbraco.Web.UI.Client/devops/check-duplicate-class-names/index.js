/**
 * This module detects duplicate TypeScript class names in the Umbraco backoffice.
 * It scans for class declarations and reports any duplicates found.
 * @example node devops/check-duplicate-class-names/index.js src
 * @author Lee Kelleher, (using Claude Code)
 */

import { readFileSync, readdirSync, statSync } from 'fs';
import { join, relative } from 'path';

const IS_GITHUB_ACTIONS = process.env.GITHUB_ACTIONS === 'true';
const IS_AZURE_PIPELINES = process.env.TF_BUILD === 'true';

const baseDir = process.argv[2] || 'src';
const ignoreTestFiles = process.argv.includes('--ignore-tests');
const ignoreStoryFiles = process.argv.includes('--ignore-stories');

console.log('Scanning for duplicate class names in:', baseDir);
if (ignoreTestFiles) {
	console.log('(Ignoring test files)');
}
if (ignoreStoryFiles) {
	console.log('(Ignoring story files)');
}
console.log('-'.repeat(80));

/**
 * Recursively find all TypeScript files in a directory.
 * @param {string} dir - Directory to search.
 * @param {string[]} files - Accumulator for found files.
 * @returns {string[]} Array of file paths.
 */
function findTypeScriptFiles(dir, files = []) {
	const entries = readdirSync(dir);

	for (const entry of entries) {
		const fullPath = join(dir, entry);
		const stat = statSync(fullPath);

		if (stat.isDirectory()) {
			// Skip node_modules and dist directories
			if (entry !== 'node_modules' && !entry.startsWith('dist')) {
				findTypeScriptFiles(fullPath, files);
			}
		} else if (entry.endsWith('.ts') && !entry.endsWith('.d.ts')) {
			// Skip test files if flag is set
			if (ignoreTestFiles && (entry.endsWith('.test.ts') || entry.endsWith('.spec.ts'))) {
				continue;
			}
			// Skip story files if flag is set
			if (ignoreStoryFiles && entry.endsWith('.stories.ts')) {
				continue;
			}
			files.push(fullPath);
		}
	}

	return files;
}

/**
 * Extract class names from a TypeScript file.
 * @param {string} filePath - Path to the file.
 * @returns {{ className: string, filePath: string }[]} Array of class declarations.
 */
function extractClassNames(filePath) {
	const content = readFileSync(filePath, 'utf-8');
	const classDeclarations = [];

	// Match exported class declarations
	// Patterns: "export class ClassName", "export abstract class ClassName", "export default class ClassName", "export default abstract class ClassName"
	const regex = /export\s+(?:default\s+)?(?:abstract\s+)?class\s+(\w+)/g;
	let match;

	while ((match = regex.exec(content)) !== null) {
		classDeclarations.push({
			className: match[1],
			filePath: relative(process.cwd(), filePath).replace(/\\/g, '/'),
		});
	}

	return classDeclarations;
}

/**
 * Find duplicate class names.
 * @param {{ className: string, filePath: string }[]} declarations - All class declarations.
 * @returns {Map<string, string[]>} Map of class name to file paths (only duplicates).
 */
function findDuplicates(declarations) {
	/** @type {Map<string, string[]>} */
	const classMap = new Map();

	for (const { className, filePath } of declarations) {
		if (!classMap.has(className)) {
			classMap.set(className, []);
		}
		classMap.get(className).push(filePath);
	}

	// Filter to only duplicates
	/** @type {Map<string, string[]>} */
	const duplicates = new Map();
	for (const [className, files] of classMap) {
		if (files.length > 1) {
			duplicates.set(className, files);
		}
	}

	return duplicates;
}

/**
 * Print a duplicate class name issue.
 * @param {string} className - The duplicated class name.
 * @param {string[]} files - Files containing the duplicate.
 * @param {number} idx - Index for display.
 */
function printDuplicate(className, files, idx) {
	const fileList = files.join(', ');

	if (IS_GITHUB_ACTIONS) {
		console.log(
			`::error file=${files[0]},title=Duplicate class name::Class "${className}" is defined in multiple files: ${fileList}`,
		);
	} else if (IS_AZURE_PIPELINES) {
		console.log(
			`##vso[task.logissue type=error;sourcepath=${files[0]};]Class "${className}" is defined in multiple files: ${fileList}`,
		);
	} else {
		console.log(`${idx}. ${className}`);
		for (const file of files) {
			console.log(`   - ${file}`);
		}
		console.log('');
	}
}

// Main execution
const tsFiles = findTypeScriptFiles(baseDir);
console.log(`Found ${tsFiles.length} TypeScript files to scan.\n`);

const allDeclarations = [];
for (const file of tsFiles) {
	const declarations = extractClassNames(file);
	allDeclarations.push(...declarations);
}

console.log(`Found ${allDeclarations.length} class declarations.\n`);

const duplicates = findDuplicates(allDeclarations);

if (duplicates.size > 0) {
	console.log(`${duplicates.size} duplicate class name(s) detected:\n`);

	let idx = 1;
	for (const [className, files] of duplicates) {
		printDuplicate(className, files, idx++);
	}

	console.log('Please fix the duplicate class names before proceeding.\n');
	process.exit(1);
}

console.log('No duplicate class names detected.\n');
process.exit(0);
