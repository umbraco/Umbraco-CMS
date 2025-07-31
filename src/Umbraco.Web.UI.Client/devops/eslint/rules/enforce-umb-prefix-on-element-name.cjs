/** @type {import('eslint').Rule.RuleModule} */
module.exports = {
	meta: {
		type: 'suggestion',
		docs: {
			description: 'Enforce Custom Element names to start with "umb-".',
			category: 'Naming',
			recommended: true,
		},
		schema: [],
	},
	create: function (context) {
		return {
			CallExpression(node) {
				// check if the expression is @customElement decorator
				const isCustomElementDecorator =
					node.callee.type === 'Identifier' &&
					node.callee.name === 'customElement' &&
					node.arguments.length === 1 &&
					node.arguments[0].type === 'Literal' &&
					typeof node.arguments[0].value === 'string';

				if (isCustomElementDecorator) {
					const elementName = node.arguments[0].value;

					// check if the element name starts with 'umb-', 'ufm-', or 'test-', to be allow tests to have custom elements:
					const prefixes = ['umb-', 'ufm-', 'test-'];
					const isElementNameValid = prefixes.some((prefix) => elementName.startsWith(prefix));

					if (!isElementNameValid) {
						context.report({
							node,
							message: 'Custom Element name should start with "umb-" or "ufm-".',
							// There is no fixer on purpose because it's not safe to automatically rename the element name.
							// Renaming should be done manually with consideration of potential impacts.
						});
					}
				}
			},
		};
	},
};
