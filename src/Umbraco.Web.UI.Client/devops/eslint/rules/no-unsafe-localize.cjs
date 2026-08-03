'use strict';

/**
 * Flags two XSS-prone shapes:
 *
 * 1. `unsafeHTML(<x>.localize.string(...))` / `unsafeHTML(<x>.localize.term(...))` — wrapping a
 *    localized string in `unsafeHTML` leaves any interpolated args un-escaped. Use
 *    `<x>.localize.htmlString(text, ...args)` instead, which escapes args via escapeHTML and
 *    returns a Lit unsafeHTML directive ready to inline in templates.
 *
 * 2. `umbConfirmModal(_, { content: ... })` (and other modals sharing the same `content` sink)
 *    given a `string` built from a template literal with interpolation, or an unescaped
 *    `<x>.localize.string|term(key, ...args)` call — the modal renders string `content` via
 *    `unsafeHTML` internally, so any interpolated arg reaches the DOM unescaped. Use
 *    `content: html\`${this.localize.htmlString(key, ...args)}\`` (or a plain `html` template,
 *    for keys with no HTML markup) instead.
 *
 * See `docs/security.md` (XSS Prevention → Localized HTML) for the full pattern.
 */

/** @type {import('eslint').Rule.RuleModule} */
module.exports = {
	meta: {
		type: 'problem',
		docs: {
			description:
				'Disallow `unsafeHTML(<x>.localize.string|term(...))` and unescaped interpolation in modal `content`.',
			category: 'Possible Errors',
			recommended: true,
		},
		schema: [],
		messages: {
			unsafeLocalize:
				'Avoid `unsafeHTML(...localize.{{method}}(...))` — interpolated args are not escaped (XSS hazard). Use `localize.htmlString(...)` instead.',
			unsafeModalContent:
				'Avoid interpolating values directly into modal `content` — it is rendered via `unsafeHTML` (XSS hazard). Wrap the value in an `html` template (or use `localize.htmlString(key, ...args)` for keys containing markup).',
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

		/**
		 * Returns true if an arg to `<localizeExpr>.string|term(key, ...args)` is provably harmless
		 * regardless of its runtime value: a literal, a `<expr>.length` access (always a number), an
		 * `escapeHTML(...)` call (already sanitized), or an array of only such values.
		 */
		function isKnownSafeArg(argNode) {
			if (!argNode) return false;

			if (argNode.type === 'Literal') return true;

			if (argNode.type === 'MemberExpression' && argNode.property?.type === 'Identifier' && argNode.property.name === 'length') {
				return true;
			}

			if (argNode.type === 'CallExpression' && argNode.callee?.type === 'Identifier' && argNode.callee.name === 'escapeHTML') {
				return true;
			}

			if (argNode.type === 'ArrayExpression') {
				return argNode.elements.every((el) => isKnownSafeArg(el));
			}

			return false;
		}

		/**
		 * Returns true if the given `content:` value is provably unsafe: a template literal with at
		 * least one interpolated expression, or an `<localizeExpr>.string|term(key, ...args)` call
		 * where at least one arg beyond the key isn't provably harmless. `html`...`` (a
		 * TaggedTemplateExpression), static strings, arg-less localize calls, calls where every extra
		 * arg is known-safe (see `isKnownSafeArg`), and opaque variables are left alone.
		 */
		function isUnsafeContentValue(valueNode) {
			if (!valueNode) return false;

			if (valueNode.type === 'TemplateLiteral') {
				return valueNode.expressions.length > 0;
			}

			if (valueNode.type === 'CallExpression') {
				const callee = valueNode.callee;
				if (!callee || callee.type !== 'MemberExpression') return false;
				if (callee.property?.type !== 'Identifier') return false;

				const method = callee.property.name;
				if (method !== 'string' && method !== 'term') return false;
				if (!isLocalizeMemberAccess(callee.object)) return false;

				const extraArgs = valueNode.arguments.slice(1);
				if (extraArgs.length === 0) return false;

				return extraArgs.some((arg) => !isKnownSafeArg(arg));
			}

			return false;
		}

		return {
			CallExpression(node) {
				if (!node.callee || node.callee.type !== 'Identifier') return;

				// Looking for: unsafeHTML(<inner>)
				if (node.callee.name === 'unsafeHTML') {
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
					return;
				}

				// Looking for: umbConfirmModal(<host>, { content: <unsafe> }) or umbInfoModal(<host>, { content: <unsafe> })
				if (node.callee.name === 'umbConfirmModal' || node.callee.name === 'umbInfoModal') {
					const dataArg = node.arguments[1];
					if (!dataArg || dataArg.type !== 'ObjectExpression') return;

					const contentProp = dataArg.properties.find(
						(p) => p.type === 'Property' && p.key?.type === 'Identifier' && p.key.name === 'content',
					);
					if (!contentProp) return;

					if (isUnsafeContentValue(contentProp.value)) {
						context.report({
							node: contentProp,
							messageId: 'unsafeModalContent',
						});
					}
				}
			},
		};
	},
};
