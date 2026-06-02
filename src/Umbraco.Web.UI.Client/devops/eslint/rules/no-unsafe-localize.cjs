'use strict';

/**
 * Flags `unsafeHTML(<x>.localize.string(...))` and `unsafeHTML(<x>.localize.term(...))`.
 *
 * Wrapping a localized string in `unsafeHTML` leaves any interpolated args un-escaped — an XSS hazard
 * when the args are user-controlled. Use `<x>.localize.htmlString(text, ...args)` instead, which
 * escapes args via escapeHTML and returns a Lit unsafeHTML directive ready to inline in templates.
 *
 * See `docs/security.md` (XSS Prevention → Localized HTML) for the full pattern.
 */

/** @type {import('eslint').Rule.RuleModule} */
module.exports = {
	meta: {
		type: 'problem',
		docs: {
			description: 'Disallow `unsafeHTML(<x>.localize.string|term(...))`. Use `<x>.localize.htmlString(...)` instead.',
			category: 'Possible Errors',
			recommended: true,
		},
		schema: [],
		messages: {
			unsafeLocalize:
				'Avoid `unsafeHTML(...localize.{{method}}(...))` — interpolated args are not escaped (XSS hazard). Use `localize.htmlString(...)` instead.',
		},
	},
	create(context) {
		/**
		 * Returns true if the given AST node represents a `<...>.localize` member access,
		 * e.g. `this.localize`, `this.#localize`, `host.localize`, `this._localize`.
		 */
		function isLocalizeMemberAccess(node) {
			if (!node || node.type !== 'MemberExpression') return false;
			const prop = node.property;
			if (!prop) return false;
			// Match `localize` (regular) or any private/aliased identifier ending in `localize` (e.g. `#localize`, `_localize`).
			if (prop.type === 'Identifier' && /localize$/i.test(prop.name)) return true;
			if (prop.type === 'PrivateIdentifier' && /localize$/i.test(prop.name)) return true;
			return false;
		}

		return {
			CallExpression(node) {
				// Looking for: unsafeHTML(<inner>)
				if (!node.callee || node.callee.type !== 'Identifier' || node.callee.name !== 'unsafeHTML') return;
				if (node.arguments.length === 0) return;

				const arg = node.arguments[0];
				if (!arg || arg.type !== 'CallExpression') return;

				// <inner> must itself be a member-call: <localizeExpr>.string(...) or .term(...)
				const innerCallee = arg.callee;
				if (!innerCallee || innerCallee.type !== 'MemberExpression') return;
				if (innerCallee.property?.type !== 'Identifier') return;

				const method = innerCallee.property.name;
				if (method !== 'string' && method !== 'term') return;

				if (!isLocalizeMemberAccess(innerCallee.object)) return;

				context.report({
					node,
					messageId: 'unsafeLocalize',
					data: { method },
				});
			},
		};
	},
};
