// TODO: figure out how to automatically generate this list of module paths
const modulePathIdentifiers = [
	'/core/action/',
	'/core/audit-log/',
	'/core/auth/',
	'/core/collection/',
	'/core/components/',
	'/core/content-type/',
	'/core/content/',
	'/core/culture/',
	'/core/debug/',
	'/core/entity-action/',
	'/core/entity-bulk-action/',
	'/core/entity/',
	'/core/event/',
	'/core/extension-registry/',
	'/core/icon-registry/',
	'/core/id/',
	'/core/lit-element/',
	'/core/localization/',
	'/core/menu/',
	'/core/modal/',
	'/core/models/',
	'/core/notification/',
	'/core/picker-input/',
	'/core/property/',
	'/core/property-editor/',
	'/core/recycle-bin/',
	'/core/repository/',
	'/core/resources/',
	'/core/router/',
	'/core/section/',
	'/core/server-file-system/',
	'/core/settings/',
	'/core/sorter/',
	'/core/store/',
	'/core/style/',
	'/core/temporary-file/',
	'/core/themes/',
	'/core/tree/',
	'/core/utils/',
	'/core/validation/',
	'/core/variant/',
	'/core/workspace/',
	'/class-api/',
	'/context-api/',
	'/controller-api/',
	'/element-api/',
	'/extension-api/',
	'/formatting-api/',
	'/localization-api/',
	'/observable-api/',
	'/backend-api/',
	'/base64-js/',
	'/diff/',
	'/dompurify/',
	'/lit/',
	'/marked/',
	'/monaco-editor/',
	'/openid/',
	'/router-slot/',
	'/rxjs/',
	'/tinymce/',
	'/uui/',
	'/uuid/',
];

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
