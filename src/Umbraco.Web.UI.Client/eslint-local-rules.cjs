'use strict';

/*
 * A eslint rule that ensures the use of the `import type` operator from the `src/core/models/index.ts` file.
 */
// eslint-disable-next-line no-undef
module.exports = {
	/** @type {import('eslint').Rule.RuleModule} */
	'bad-type-import': {
		meta: {
			type: 'problem',
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
					if (
						node.source.parent.importKind !== 'type' &&
						(node.source.value.endsWith('/models') || node.source.value === 'router-slot/model')
					) {
						const sourceCode = context.getSourceCode();
						const nodeSource = sourceCode.getText(node);
						context.report({
							node,
							message: 'Use `import type` instead of `import`.',
							fix: (fixer) => fixer.replaceText(node, nodeSource.replace('import', 'import type')),
						});
					}
				},
			};
		},
	},

	/** @type {import('eslint').Rule.RuleModule} */
	'no-direct-api-import': {
		meta: {
			type: 'suggestion',
			docs: {
				description:
					'Ensures that any API resources from the `@umbraco-cms/backoffice/backend-api` module are not used directly. Instead you should use the `tryExecuteAndNotify` function from the `@umbraco-cms/backoffice/resources` module.',
				category: 'Best Practices',
				recommended: true,
			},
			fixable: 'code',
			schema: [],
		},
		create: function (context) {
			return {
				// If methods called on *Resource classes are not already wrapped with `await tryExecuteAndNotify()`, then we should suggest to wrap them.
				CallExpression: function (node) {
					if (
						node.callee.type === 'MemberExpression' &&
						node.callee.object.type === 'Identifier' &&
						node.callee.object.name.endsWith('Resource') &&
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
								message: 'Wrap this call with `tryExecuteAndNotify()`. Make sure to `await` the result.',
								fix: (fixer) => [
									fixer.insertTextBefore(node, 'tryExecuteAndNotify(this, '),
									fixer.insertTextAfter(node, ')'),
								],
							});
						}
					}
				},
			};
		},
	},

	/** @type {import('eslint').Rule.RuleModule} */
	'prefer-import-aliases': {
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
	},

	/** @type {import('eslint').Rule.RuleModule} */
	'enforce-element-suffix-on-element-class-name': {
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
	},

	/** @type {import('eslint').Rule.RuleModule} */
	/*
	'no-external-imports': {
		meta: {
			type: 'problem',
			docs: {
				description:
					'Ensures that the application does not rely on imports from external packages. Instead, use the @umbraco-cms/backoffice libs.',
				recommended: true,
			},
			fixable: 'code',
			schema: [],
		},
		create: function (context) {
			return {
				ImportDeclaration: function (node) {
					// Check for imports from "router-slot"
					if (node.source.value.startsWith('router-slot')) {
						context.report({
							node,
							message:
								'Use the `@umbraco-cms/backoffice/router` package instead of importing directly from "router-slot" because we might change that dependency in the future.',
							fix: (fixer) => {
								return fixer.replaceTextRange(node.source.range, `'@umbraco-cms/backoffice/router'`);
							},
						});
					}
				},
			};
		},
	},
	*/

	/** @type {import('eslint').Rule.RuleModule} */
	'umb-class-prefix': {
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
	},

	/** @type {import('eslint').Rule.RuleModule}*/
	'prefer-static-styles-last': {
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
	},

	/** @type {import('eslint').Rule.RuleModule}*/
	'ensure-relative-import-use-js-extension': {
		meta: {
			type: 'problem',
			docs: {
				description: 'Ensures relative imports use the ".js" file extension.',
				category: 'Best Practices',
				recommended: true,
			},
			fixable: 'code',
			schema: [],
		},
		create: (context) => {
			function correctImport(value) {
				if (value === '.') {
					return './index.js';
				}

				if (
					value &&
					value.startsWith('.') &&
					!value.endsWith('.js') &&
					!value.endsWith('.css') &&
					!value.endsWith('.json') &&
					!value.endsWith('.svg') &&
					!value.endsWith('.jpg') &&
					!value.endsWith('.png')
				) {
					return (value.endsWith('/') ? value + 'index' : value) + '.js';
				}

				return null;
			}

			return {
				ImportDeclaration: (node) => {
					const { source } = node;
					const { value } = source;

					const fixedValue = correctImport(value);
					if (fixedValue) {
						context.report({
							node,
							message: 'Relative imports should use the ".js" file extension.',
							fix: (fixer) => fixer.replaceText(source, `'${fixedValue}'`),
						});
					}
				},
				ImportExpression: (node) => {
					const { source } = node;
					const { value } = source;

					const fixedSource = correctImport(value);
					if (fixedSource) {
						context.report({
							node: source,
							message: 'Relative imports should use the ".js" file extension.',
							fix: (fixer) => fixer.replaceText(source, `'${fixedSource}'`),
						});
					}
				},
				ExportAllDeclaration: (node) => {
					const { source } = node;
					const { value } = source;

					const fixedSource = correctImport(value);
					if (fixedSource) {
						context.report({
							node: source,
							message: 'Relative exports should use the ".js" file extension.',
							fix: (fixer) => fixer.replaceText(source, `'${fixedSource}'`),
						});
					}
				},
				ExportNamedDeclaration: (node) => {
					const { source } = node;
					if (!source) return;
					const { value } = source;

					const fixedSource = correctImport(value);
					if (fixedSource) {
						context.report({
							node: source,
							message: 'Relative exports should use the ".js" file extension.',
							fix: (fixer) => fixer.replaceText(source, `'${fixedSource}'`),
						});
					}
				}
			};
		},
	},
};
