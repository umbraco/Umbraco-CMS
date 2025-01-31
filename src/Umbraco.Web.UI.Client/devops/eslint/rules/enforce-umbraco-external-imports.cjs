/** @type {import('eslint').Rule.RuleModule}*/
module.exports = {
	meta: {
		type: 'problem',
		docs: {
			description:
				'Ensures that the application strictly uses node_modules imports from `@umbraco-cms/backoffice/external`. This is needed to run the application in the browser.',
			recommended: true,
		},
		fixable: 'code',
		schema: {
			type: 'array',
			minItems: 0,
			maxItems: 1,
			items: [
				{
					type: 'object',
					properties: {
						exceptions: { type: 'array' },
					},
					additionalProperties: false,
				},
			],
		},
	},
	create: (context) => {
		return {
			ImportDeclaration: (node) => {
				const { source } = node;
				const { value } = source;

				const options = context.options[0] || {};
				const exceptions = options.exceptions || [];

				// If import starts with any of the following, then it's allowed
				if (exceptions.some((v) => value.startsWith(v))) {
					return;
				}

				context.report({
					node,
					message:
						'node_modules imports should be proxied through `@umbraco-cms/backoffice/external`. Please create it if it does not exist.',
					fix: (fixer) =>
						fixer.replaceText(source, `'@umbraco-cms/backoffice/external${value.startsWith('/') ? '' : '/'}${value}'`),
				});
			},
		};
	},
};
