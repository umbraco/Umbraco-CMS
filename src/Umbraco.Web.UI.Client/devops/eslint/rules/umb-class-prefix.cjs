/** @type {import('eslint').Rule.RuleModule} */
module.exports = {
	meta: {
		type: 'problem',
		docs: {
			description: 'Ensure that all class declarations are prefixed with "Umb"',
			category: 'Best Practices',
			recommended: true,
		},
		schema: [],
	},
	create: function (context) {
		function checkClassName(node) {
			if (node.id && node.id.name && !node.id.name.startsWith('Umb')) {
				context.report({
					node: node.id,
					message: 'Class declaration should be prefixed with "Umb"',
				});
			}
		}

		return {
			ClassDeclaration: checkClassName,
		};
	},
};
