/** @type {import('eslint').Rule.RuleModule} */
module.exports = {
	meta: {
		type: 'suggestion',
		docs: {
			description: 'Enforce Element class name to end with "Element".',
			category: 'Naming',
			recommended: true,
		},
		schema: [],
	},
	create: function (context) {
		return {
			ClassDeclaration(node) {
				// check if the class extends HTMLElement, LitElement, or UmbLitElement
				const isExtendingElement =
					node.superClass && ['HTMLElement', 'LitElement', 'UmbLitElement'].includes(node.superClass.name);
				// check if the class name ends with 'Element'
				const isClassNameValid = node.id.name.endsWith('Element');

				if (isExtendingElement && !isClassNameValid) {
					context.report({
						node,
						message: "Element class name should end with 'Element'.",
						// There us no fixer on purpose because it's not safe to rename the class. We want to do that trough the refactoring tool.
					});
				}
			},
		};
	},
};
