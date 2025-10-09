/** @type {import('eslint').Rule.RuleModule} */
module.exports = {
	meta: {
		docs: {
			description:
				'Ensures that any API resources from the `@umbraco-cms/backoffice/external/backend-api` module are not used directly. Instead you should use the `tryExecuteAndNotify` function from the `@umbraco-cms/backoffice/resources` module.',
			category: 'Best Practices',
			recommended: true,
		},
		fixable: 'code',
		schema: [],
	},
	create: function (context) {
		return {
			// If methods called on *Service classes are not already wrapped with `await tryExecute()`, then we should suggest to wrap them.
			CallExpression: function (node) {
				if (
					node.callee.type === 'MemberExpression' &&
					node.callee.object.type === 'Identifier' &&
					node.callee.object.name.endsWith('Service') &&
					node.callee.property.type === 'Identifier' &&
					node.callee.property.name !== 'constructor'
				) {
					const hasTryExecuteAndNotify =
						node.parent &&
						node.parent.callee &&
						(node.parent.callee.name === 'tryExecute' || node.parent.callee.name === 'tryExecuteAndNotify');
					if (!hasTryExecuteAndNotify) {
						context.report({
							node,
							message: 'Wrap this call with `tryExecute()`. Make sure to `await` the result.',
							fix: (fixer) => [
								fixer.insertTextBefore(node, 'tryExecute(this, '),
								fixer.insertTextAfter(node, ')'),
							],
						});
					}
				}
			},
		};
	},
};
