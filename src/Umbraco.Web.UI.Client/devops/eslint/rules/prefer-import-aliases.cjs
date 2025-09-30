/** @type {import('eslint').Rule.RuleModule} */
module.exports = {
	meta: {
		type: 'suggestion',
		docs: {
			description:
				'Ensures that the application does not rely on file system paths for imports. Instead, use import aliases or relative imports. This also solves a problem where GitHub fails on the test runner step.',
			category: 'Best Practices',
			recommended: true,
		},
		schema: [],
	},
	create: function (context) {
		return {
			ImportDeclaration: function (node) {
				if (node.source.value.startsWith('src/')) {
					context.report({
						node,
						message:
							'Prefer using import aliases or relative imports instead of absolute imports. Example: `import { MyComponent } from "src/components/MyComponent";` should be `import { MyComponent } from "@components/MyComponent";`',
					});
				}
			},
		};
	},
};
