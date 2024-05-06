/**
 * This script is used to compare the keys in the localization files. It will take a main language (en.js) and compare with the other languages.
 * The script will output the keys that are missing in the other languages and the keys that are missing in the main language.
 *
 * Note: Since the source files are TypeScript files, the script will only compare on the dist-cms files.
 *
 * Usage: node devops/localization/compare-languages.js [filter]
 * Example: node devops/localization/compare-languages.js da-dk.js
 *
 * Copyright (c) 2024 by Umbraco HQ
 */

import fs from 'fs';
import path from 'path';

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

const filter = process.argv[2];
if (filter) {
	console.log(`Filtering on: ${filter}`);
}

const languages = fs.readdirSync(languageFolder).filter((file) => file !== mainLanguage && file.endsWith('.js') && (!filter || file.includes(filter)));
const missingKeysInMain = [];

const languagePromise = Promise.all(languages.map(async (language) => {
	const languageKeys = (await import(path.join(languageFolder, language))).default;
	const languageMap = buildMap(languageKeys);

	const missingKeys = Array.from(mainMap.keys()).filter((key) => !languageMap.has(key));
	let localMissingKeysInMain = Array.from(languageMap.keys()).filter((key) => !mainMap.has(key));
	localMissingKeysInMain = localMissingKeysInMain.map((key) => `${key} (${language})`);
	missingKeysInMain.push(...localMissingKeysInMain);

	console.log(`\n${language}:`);
	console.log(`Missing keys in ${language}:`);
	console.log(missingKeys);
}));

await languagePromise;

console.log(`Missing keys in ${mainLanguage}:`);
console.log(missingKeysInMain);

function buildMap(keys) {
	const map = new Map();

	for (const key in keys) {
		for (const subKey in keys[key]) {
			map.set(`${key}_${subKey}`, keys[key][subKey]);
		}
	}

	return map;
}
