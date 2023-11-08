/** @type {import('eslint').Rule.RuleModule}*/
module.exports = {
	meta: {
		type: 'suggestion',
		docs: {
			description:
				'Enforce the "styles" property with the static modifier to be the last property of a class that ends with "Element".',
			category: 'Best Practices',
			recommended: true,
		},
		fixable: 'code',
		schema: [],
	},
	create: function (context) {
		return {
			ClassDeclaration(node) {
				const className = node.id.name;
				if (className.endsWith('Element')) {
					const staticStylesProperty = node.body.body.find((bodyNode) => {
						return bodyNode.type === 'PropertyDefinition' && bodyNode.key.name === 'styles' && bodyNode.static;
					});
					if (staticStylesProperty) {
						const lastProperty = node.body.body[node.body.body.length - 1];
						if (lastProperty.key.name !== staticStylesProperty.key.name) {
							context.report({
								node: staticStylesProperty,
								message: 'The "styles" property should be the last property of a class declaration.',
								data: {
									className: className,
								},
								fix: function (fixer) {
									const sourceCode = context.getSourceCode();
									const staticStylesPropertyText = sourceCode.getText(staticStylesProperty);
									return [
										fixer.replaceTextRange(staticStylesProperty.range, ''),
										fixer.insertTextAfterRange(lastProperty.range, '\n	\n	' + staticStylesPropertyText),
									];
								},
							});
						}
					}
				}
			},
		};
	},
};
