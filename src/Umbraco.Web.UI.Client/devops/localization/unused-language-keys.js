/**
 * This script is used to find unused language keys in the javascript files. It will take a main language (en.js) and compare with the other languages.
 *
 * Usage: node devops/localization/unused-language-keys.js
 * Example: node devops/localization/unused-language-keys.js
 *
 * Copyright (c) 2024 by Umbraco HQ
 */
import fs from 'fs';
import path from 'path';
import glob from 'tiny-glob';

const mainLanguage = 'en.js';

const __dirname = import.meta.dirname;
const languageFolder = path.join(__dirname, '../../dist-cms/assets/lang');

// Check that the languageFolder exists
if (!fs.existsSync(languageFolder)) {
	console.error(`The language folder does not exist: ${languageFolder}. You need to build the project first by running 'npm run build'`);
	process.exit(1);
}

const mainKeys = (await import(path.join(languageFolder, mainLanguage))).default;
const mainMap = buildMap(mainKeys);
const keys = Array.from(mainMap.keys());
const usedKeys = new Set();

const elementAndControllerFiles = await glob(`${__dirname}/../../src/**/*.ts`, { filesOnly: true });

console.log(`Checking ${elementAndControllerFiles.length} files for unused keys`);

// Find all the keys used in the javascript files
const filePromise = Promise.all(elementAndControllerFiles.map(async (file) => {
	// Check if each key is in the file (simple)
	const fileContent = fs.readFileSync(file, 'utf8');
	keys.forEach((key) => {
		if (fileContent.includes(key)) {
			usedKeys.add(key);
		}
	});
}));

await filePromise;

const unusedKeys = Array.from(mainMap.keys()).filter((key) => !usedKeys.has(key));

console.log(`\n${mainLanguage}:`);
console.log(`Used keys in ${mainLanguage}:`);
console.log(usedKeys);
console.log(`Unused keys in ${mainLanguage}:`);
console.log(unusedKeys);

function buildMap(keys) {
	const map = new Map();

	for (const key in keys) {
		for (const subKey in keys[key]) {
			map.set(`${key}_${subKey}`, keys[key][subKey]);
		}
	}

	return map;
}
