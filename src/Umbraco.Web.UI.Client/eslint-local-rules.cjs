'use strict';

/*
 * A eslint rule that ensures the use of the `import type` operator from the `src/core/models/index.ts` file.
 */
// eslint-disable-next-line no-undef
module.exports = {
	/** @type {import('eslint').Rule.RuleModule} */
	'bad-type-import': {
		meta: {
			type: 'suggestion',
			docs: {
				description: 'Ensures the use of the `import type` operator from the `src/core/models/index.ts` file.',
				category: 'Best Practices',
				recommended: true,
			},
			fixable: 'code',
			schema: [],
		},
		create: function (context) {
			return {
				ImportDeclaration: function (node) {
					if (node.source.parent.importKind !== 'type' && (node.source.value.endsWith('/models') || node.source.value === 'router-slot/model')) {
						const sourceCode = context.getSourceCode();
						const nodeSource = sourceCode.getText(node);
						context.report({
							node,
							message: 'Use `import type` instead of `import`.',
							fix: fixer => fixer.replaceText(node, nodeSource.replace('import', 'import type')),
						});
					}
				},
			};
		}
	},

	/** @type {import('eslint').Rule.RuleModule} */
	'no-direct-api-import': {
		meta: {
			type: 'suggestion',
			docs: {
				description: 'Ensures that any API resources from the `@umbraco-cms/backend-api` module are not used directly. Instead you should use the `tryExecuteAndNotify` function from the `@umbraco-cms/resources` module.',
				category: 'Best Practices',
				recommended: true
			},
			fixable: 'code',
			schema: [],
		},
		create: function (context) {
			return {
				// If methods called on *Resource classes are not already wrapped with `await tryExecuteAndNotify()`, then we should suggest to wrap them.
				CallExpression: function (node) {
					if (node.callee.type === 'MemberExpression' && node.callee.object.type === 'Identifier' && node.callee.object.name.endsWith('Resource') && node.callee.property.type === 'Identifier' && node.callee.property.name !== 'constructor') {
						const hasTryExecuteAndNotify = node.parent.callee.name === 'tryExecuteAndNotify';
						if (!hasTryExecuteAndNotify) {
							context.report({
								node,
								message: 'Wrap this call with `tryExecuteAndNotify()`. Make sure to `await` the result.',
								fix: fixer => [fixer.insertTextBefore(node, 'tryExecuteAndNotify(this, '), fixer.insertTextAfter(node, ')')],
							});
						}
					}
				}
			};

		},
	}
};
