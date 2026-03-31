'use strict';

/**
 * Registry tracking alias occurrences across files within a single ESLint run.
 * Used for cross-file uniqueness checks.
 * @type {Map<string, {file: string, line: number}>}
 */
const aliasRegistry = new Map();

/**
 * Check if a string is PascalCase: starts with uppercase letter, only alphanumeric characters.
 * @param {string} str
 * @returns {boolean}
 */
function isPascalCase(str) {
	return /^[A-Z][a-zA-Z0-9]*$/.test(str);
}

/** @type {import('eslint').Rule.RuleModule} */
module.exports = {
	meta: {
		type: 'problem',
		docs: {
			description:
				'Enforce unique manifest aliases in Umb.PascalCase.PascalCase format with at least 3 dot-separated segments.',
			category: 'Naming',
			recommended: true,
		},
		schema: [],
		messages: {
			duplicateAlias:
				'Duplicate manifest alias "{{alias}}". First defined in {{firstFile}}:{{firstLine}}.',
			missingUmbPrefix: 'Manifest alias "{{alias}}" must start with "Umb.".',
			tooFewSegments:
				'Manifest alias "{{alias}}" must have at least 3 dot-separated segments (e.g., "Umb.Type.Name").',
			invalidSegment: 'Alias segment "{{segment}}" in "{{alias}}" must be PascalCase (e.g., "MySegment").',
		},
	},
	create(context) {
		const filename = context.filename || context.getFilename();

		// Clear stale entries from this file to handle re-linting in watch mode / editor integrations.
		for (const [alias, info] of aliasRegistry) {
			if (info.file === filename) {
				aliasRegistry.delete(alias);
			}
		}

		return {
			ObjectExpression(node) {
				const aliasProperty = node.properties.find(
					(p) =>
						p.type === 'Property' &&
						!p.computed &&
						((p.key.type === 'Identifier' && p.key.name === 'alias') ||
							(p.key.type === 'Literal' && p.key.value === 'alias')),
				);
				const typeProperty = node.properties.find(
					(p) =>
						p.type === 'Property' &&
						!p.computed &&
						((p.key.type === 'Identifier' && p.key.name === 'type') ||
							(p.key.type === 'Literal' && p.key.value === 'type')),
				);

				// Only validate objects that look like manifest declarations (have both type and alias).
				if (!aliasProperty || !typeProperty) return;

				// Only validate string literal alias values; constant references can't be statically resolved here.
				if (aliasProperty.value.type !== 'Literal' || typeof aliasProperty.value.value !== 'string') return;

				const alias = aliasProperty.value.value;
				const line = aliasProperty.value.loc?.start.line ?? 0;

				// --- Uniqueness check ---
				const existing = aliasRegistry.get(alias);
				if (existing) {
					if (existing.file !== filename || existing.line !== line) {
						context.report({
							node: aliasProperty.value,
							messageId: 'duplicateAlias',
							data: {
								alias,
								firstFile: existing.file,
								firstLine: String(existing.line),
							},
						});
					}
				} else {
					aliasRegistry.set(alias, { file: filename, line });
				}

				// --- Format validation ---
				const segments = alias.split('.');

				if (segments[0] !== 'Umb') {
					context.report({
						node: aliasProperty.value,
						messageId: 'missingUmbPrefix',
						data: { alias },
					});
					return;
				}

				if (segments.length < 3) {
					context.report({
						node: aliasProperty.value,
						messageId: 'tooFewSegments',
						data: { alias },
					});
					return;
				}

				// Every segment must be PascalCase.
				for (const segment of segments) {
					if (!isPascalCase(segment)) {
						context.report({
							node: aliasProperty.value,
							messageId: 'invalidSegment',
							data: { segment, alias },
						});
						break;
					}
				}
			},
		};
	},
};
