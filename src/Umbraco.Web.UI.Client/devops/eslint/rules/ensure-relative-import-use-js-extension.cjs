/** @type {import('eslint').Rule.RuleModule}*/
module.exports = {
	meta: {
		type: 'problem',
		docs: {
			description: 'Ensures relative imports use the ".js" file extension.',
			category: 'Best Practices',
			recommended: true,
		},
		fixable: 'code',
		schema: [],
	},
	create: (context) => {
		function correctImport(value) {
			if (value === '.') {
				return './index.js';
			}

			if (
				value &&
				value.startsWith('.') &&
				!value.endsWith('.js') &&
				!value.endsWith('.css') &&
				!value.endsWith('.json') &&
				!value.endsWith('.svg') &&
				!value.endsWith('.jpg') &&
				!value.endsWith('.png')
			) {
				return (value.endsWith('/') ? value + 'index' : value) + '.js';
			}

			return null;
		}

		return {
			ImportDeclaration: (node) => {
				const { source } = node;
				const { value } = source;

				const fixedValue = correctImport(value);
				if (fixedValue) {
					context.report({
						node,
						message: 'Relative imports should use the ".js" file extension.',
						fix: (fixer) => fixer.replaceText(source, `'${fixedValue}'`),
					});
				}
			},
			ImportExpression: (node) => {
				const { source } = node;
				const { value } = source;

				const fixedSource = correctImport(value);
				if (fixedSource) {
					context.report({
						node: source,
						message: 'Relative imports should use the ".js" file extension.',
						fix: (fixer) => fixer.replaceText(source, `'${fixedSource}'`),
					});
				}
			},
			ExportAllDeclaration: (node) => {
				const { source } = node;
				const { value } = source;

				const fixedSource = correctImport(value);
				if (fixedSource) {
					context.report({
						node: source,
						message: 'Relative exports should use the ".js" file extension.',
						fix: (fixer) => fixer.replaceText(source, `'${fixedSource}'`),
					});
				}
			},
			ExportNamedDeclaration: (node) => {
				const { source } = node;
				if (!source) return;
				const { value } = source;

				const fixedSource = correctImport(value);
				if (fixedSource) {
					context.report({
						node: source,
						message: 'Relative exports should use the ".js" file extension.',
						fix: (fixer) => fixer.replaceText(source, `'${fixedSource}'`),
					});
				}
			},
		};
	},
};
