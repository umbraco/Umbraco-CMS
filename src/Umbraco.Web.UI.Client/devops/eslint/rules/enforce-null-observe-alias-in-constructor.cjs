'use strict';

/**
 * @param {import('estree').Node} node
 * @returns {boolean}
 */
function isFunctionLike(node) {
	return (
		node.type === 'FunctionDeclaration' || node.type === 'FunctionExpression' || node.type === 'ArrowFunctionExpression'
	);
}

/**
 * Walks up from `node` and returns the nearest enclosing function-like node, or null if there is none.
 * @param {import('estree').Node} node
 * @returns {import('estree').Node | null}
 */
function getEnclosingFunction(node) {
	let current = node.parent;
	while (current) {
		if (isFunctionLike(current)) return current;
		current = current.parent;
	}
	return null;
}

/**
 * @param {import('estree').Node | null} functionNode
 * @returns {boolean}
 */
function isConstructor(functionNode) {
	return (
		!!functionNode &&
		functionNode.type === 'FunctionExpression' &&
		functionNode.parent?.type === 'MethodDefinition' &&
		functionNode.parent.kind === 'constructor'
	);
}

/** @type {import('eslint').Rule.RuleModule} */
module.exports = {
	meta: {
		type: 'problem',
		docs: {
			description:
				'Enforce a `null` controller alias on `this.observe()` calls made directly in a constructor, since the constructor only ever runs once and an auto-generated or reused alias can collide with another observation on the same host and silently destroy it before it ever runs.',
			category: 'Possible Errors',
			recommended: true,
		},
		fixable: 'code',
		schema: [],
		messages: {
			missingNullAlias: 'Pass `null` as the third argument to `this.observe()` when calling it directly in a constructor.',
			nonNullAlias:
				'Observations in the constructor must have `null` as the third argument. If you truly need a stable alias for manual removal, disable this rule for that line and document why.',
		},
	},
	create(context) {
		return {
			CallExpression(node) {
				const { callee } = node;
				if (
					callee.type !== 'MemberExpression' ||
					callee.object.type !== 'ThisExpression' ||
					callee.property.type !== 'Identifier' ||
					callee.property.name !== 'observe' ||
					node.arguments.length < 2
				) {
					return;
				}

				if (!isConstructor(getEnclosingFunction(node))) return;

				const thirdArg = node.arguments[2];

				if (!thirdArg) {
					const lastArg = node.arguments[node.arguments.length - 1];
					context.report({
						node,
						messageId: 'missingNullAlias',
						fix: (fixer) => fixer.insertTextAfter(lastArg, ', null'),
					});
					return;
				}

				if (thirdArg.type !== 'Literal' || thirdArg.value !== null) {
					context.report({
						node: thirdArg,
						messageId: 'nonNullAlias',
						fix: (fixer) => fixer.replaceText(thirdArg, 'null'),
					});
				}
			},
		};
	},
};
