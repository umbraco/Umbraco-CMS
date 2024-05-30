const fs = require('fs');
const path = require('path');

const getDirectories = (source) =>
	fs
		.readdirSync(source, { withFileTypes: true })
		.filter((dirent) => dirent.isDirectory())
		.map((dirent) => dirent.name);

// TODO: get the correct list of modules. This is a temporary solution where we assume that a directory is equivalent to a module
// TODO: include package modules in this list
const coreRoot = path.join(__dirname, '../../../', 'src/packages/core');
const externalRoot = path.join(__dirname, '../../../', 'src/external');
const libsRoot = path.join(__dirname, '../../../', 'src/libs');
const coreModules = getDirectories(coreRoot).map((dir) => `/core/${dir}/`);
const externalModules = getDirectories(externalRoot).map((dir) => `/${dir}/`);
const libsModules = getDirectories(libsRoot).map((dir) => `/${dir}/`);

const modulePathIdentifiers = [...coreModules, ...externalModules, ...libsModules];

/** @type {import('eslint').Rule.RuleModule} */
module.exports = {
	meta: {
		type: 'problem',
		docs: {
			description: 'Prevent relative import to a module that is in the import map.',
			category: 'Best Practices',
			recommended: true,
		},
		schema: [],
		messages: {
			unexpectedValue: 'Relative import paths should include "{{value}}".',
		},
	},
	create: function (context) {
		return {
			ImportDeclaration(node) {
				// exclude test  and story files
				if (context.filename.endsWith('.test.ts') || context.filename.endsWith('.stories.ts')) {
					return {};
				}

				const importPath = node.source.value;

				if (importPath.startsWith('./') || importPath.startsWith('../')) {
					if (modulePathIdentifiers.some((moduleName) => importPath.includes(moduleName))) {
						context.report({
							node,
							message: 'Use the correct import map alias instead of a relative import path: ' + importPath,
						});
					}
				}
			},
		};
	},
};
