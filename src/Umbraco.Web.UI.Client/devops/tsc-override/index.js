// Notice: This script is not perfect and may not work in all cases. ex. it places the override term wrong for async and setter/getter methods. But its a help any way. [NL]

import ts from 'typescript';
import path from 'node:path';
import fs from 'node:fs/promises';

const tsconfigPath = './tsconfig.json';

const cwd = process.cwd();

async function fixOverride() {
	const configFile = path.isAbsolute(tsconfigPath)
		? tsconfigPath
		: ts.findConfigFile(cwd, ts.sys.fileExists, tsconfigPath);

	if (!configFile) {
		console.error('No tsconfig file found for path:', tsconfigPath);
		process.exit(1);
	}

	const config = ts.readConfigFile(configFile, ts.sys.readFile);

	const { options, fileNames } = ts.parseJsonConfigFileContent(
		config.config,
		ts.sys,

		// Resolve to the folder where the tsconfig file located
		path.dirname(tsconfigPath),
	);

	const program = ts.createProgram({
		rootNames: fileNames,
		options,
	});

	if (fileNames.length === 0) {
		console.error('No files in the project.', {
			fileNames,
			options,
		});

		process.exit(1);
	}
	let emitResult = program.emit();

	const overrideErrors = ts
		.getPreEmitDiagnostics(program)
		.concat(emitResult.diagnostics)
		.filter((diagnostic) =>
			[
				// This member must have an 'override' modifier because it overrides a member in the base class '{0}'.
				4114,
				// This parameter property must have an 'override' modifier because it overrides a member in base class '{0}'
				4115,
				// This member must have an 'override' modifier because it overrides an abstract method that is declared in the base class '{0}'.
				4116,
			].includes(diagnostic.code),
		);

	const sortedErrors = sortErrors(overrideErrors);

	for (const diagnostic of sortedErrors) {
		await addOverride(diagnostic);
	}
}

/**
 *
 * @param {ts.Diagnostic} diagnostic
 * @returns {Promise<void>}
 */
async function addOverride(diagnostic) {
	const fileContent = (await fs.readFile(diagnostic.file.fileName, 'utf-8')).toString();
	let startIndex = diagnostic.start;

	if (fileContent.slice(0, startIndex).endsWith(' get ')) {
		startIndex -= 'get '.length;
	}
	if (fileContent.slice(0, startIndex).endsWith(' async ')) {
		startIndex -= 'async '.length;
	}
	if (fileContent.slice(0, startIndex).endsWith(' readonly ')) {
		startIndex -= 'readonly '.length;
	}

	const newFileContent = [fileContent.slice(0, startIndex), 'override ', fileContent.slice(startIndex)].join('');
	await fs.writeFile(diagnostic.file.fileName, newFileContent);
}

/**
 *
 * @param {ts.Diagnostic[]} errors
 * @returns {ts.Diagnostic[]}
 */
function sortErrors(errors) {
	// Sort by file path and start position from end to start
	// so we can insert override keyword without changing the start position of other errors in the same file that happen before
	return errors.slice(0).sort((a, b) => {
		if (a.file && b.file) {
			if (a.file.fileName === b.file.fileName) {
				return b.start - a.start;
			}

			return a.file.fileName.localeCompare(b.file.fileName);
		}

		return 0;
	});
}

fixOverride();
