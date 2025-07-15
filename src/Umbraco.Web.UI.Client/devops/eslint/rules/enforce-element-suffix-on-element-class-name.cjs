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
				// check if class is abstract
				const isAbstract = node.abstract;

				// check if the class extends HTMLElement, LitElement, or UmbLitElement
				const isExtendingElement =
					node.superClass && ['HTMLElement', 'LitElement', 'UmbLitElement'].includes(node.superClass.name);

				// check if the class name ends with 'Element' or 'ElementBase'
				const isClassNameValid = node.id.name.endsWith('Element') || isAbstract && node.id.name.endsWith('ElementBase');


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
