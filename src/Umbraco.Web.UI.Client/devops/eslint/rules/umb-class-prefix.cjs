const ALLOWED_PREFIXES = ['Umb', 'Example'];

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
			if (node.id && node.id.name && !ALLOWED_PREFIXES.some((prefix) => node.id.name.startsWith(prefix))) {
				context.report({
					node: node.id,
					message: `Class declaration should be prefixed with one of the following prefixes: ${ALLOWED_PREFIXES.join(', ')}`,
				});
			}
		}

		return {
			ClassDeclaration: checkClassName,
		};
	},
};
