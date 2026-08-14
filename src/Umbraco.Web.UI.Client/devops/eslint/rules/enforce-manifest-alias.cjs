'use strict';

const { ESLintUtils } = require('@typescript-eslint/utils');

/**
 * Registry tracking alias occurrences across files within a single ESLint run.
 * Used for cross-file uniqueness checks.
 * @type {Map<string, {file: string, line: number}>}
 */
const aliasRegistry = new Map();

/**
 * Check if a string is PascalCase: starts with uppercase letter, only alphanumeric characters.
 * Fully-uppercase abbreviations (e.g. "RTE") are allowed.
 * Mixed-case segments must not end with consecutive uppercase (e.g. "PropertyEditorUI" is invalid — use "PropertyEditorUi").
 * @param {string} str
 * @returns {boolean}
 */
function isPascalCase(str) {
	if (!/^[A-Z][a-zA-Z0-9]*$/.test(str)) return false;
	// Fully-uppercase abbreviations like "RTE" are fine.
	if (/^[A-Z]+$/.test(str)) return true;
	// In mixed-case segments, disallow trailing consecutive uppercase (e.g. "PropertyEditorUI").
	if (/[A-Z]{2,}$/.test(str)) return false;
	return true;
}

/**
 * Check if a string is a valid locale code: PascalCase parts joined by underscores or hyphens.
 * Matches patterns like "EN", "EN_US", "DA-DK", "CS_CZ", "FR_CH".
 * @param {string} str
 * @returns {boolean}
 */
function isLocaleCode(str) {
	return /^[A-Z][a-zA-Z0-9]*([_-][A-Z][a-zA-Z0-9]*)*$/.test(str);
}

/**
 * Walk a TypeScript type and its base types to check if any has the given name.
 * @param {import('typescript').Type} type
 * @param {string} name
 * @param {import('typescript').TypeChecker} checker
 * @param {Set<import('typescript').Type>} [visited]
 * @returns {boolean}
 */
function typeExtendsName(type, name, checker, visited = new Set()) {
	if (visited.has(type)) return false;
	visited.add(type);

	const symbol = type.getSymbol() || type.aliasSymbol;
	if (symbol && symbol.getName() === name) return true;

	const baseTypes = type.getBaseTypes?.() ?? [];
	for (const base of baseTypes) {
		if (typeExtendsName(base, name, checker, visited)) return true;
	}

	// Also check intersection types (Type & OtherType).
	if (type.isIntersection?.()) {
		for (const member of type.types) {
			if (typeExtendsName(member, name, checker, visited)) return true;
		}
	}

	return false;
}

/**
 * Resolve the manifest `type` value. First tries the literal property on the node,
 * then falls back to the TypeScript type's `type` property literal type.
 * @param {import('@typescript-eslint/utils').TSESTree.ObjectExpression} node
 * @param {import('typescript').TypeChecker} checker
 * @param {import('@typescript-eslint/utils').ParserServicesWithTypeInformation} services
 * @returns {string | undefined}
 */
function resolveManifestType(node, checker, services) {
	// Try literal property first.
	const typeProp = node.properties.find(
		(p) =>
			p.type === 'Property' &&
			!p.computed &&
			((p.key.type === 'Identifier' && p.key.name === 'type') ||
				(p.key.type === 'Literal' && p.key.value === 'type')),
	);
	if (typeProp && typeProp.value.type === 'Literal' && typeof typeProp.value.value === 'string') {
		return typeProp.value.value;
	}

	// Fall back to the TS type's `type` property.
	const tsNode = services.esTreeNodeToTSNodeMap.get(node);
	const objectType = checker.getTypeAtLocation(tsNode);
	const typeMember = objectType.getProperty('type');
	if (typeMember) {
		const memberType = checker.getTypeOfSymbolAtLocation(typeMember, tsNode);
		if (memberType.isStringLiteral()) {
			return memberType.value;
		}
	}

	return undefined;
}

/** @type {import('eslint').Rule.RuleModule} */
module.exports = {
	meta: {
		type: 'problem',
		docs: {
			description:
				'Enforce unique manifest aliases in Umb.PascalCase.PascalCase format with at least 3 dot-separated segments. propertyEditorSchema aliases use Umbraco prefix with 2–3 segments. Localization aliases allow locale codes (e.g., CS_CZ, DA-DK) in the last segment.',
			category: 'Naming',
			recommended: true,
		},
		schema: [],
		messages: {
			duplicateAlias:
				'Duplicate manifest alias "{{alias}}". First defined in {{firstFile}}:{{firstLine}}.',
			missingUmbPrefix: 'Manifest alias "{{alias}}" must start with "Umb.".',
			missingUmbracoPrefix:
				'propertyEditorSchema alias "{{alias}}" must start with "Umbraco.".',
			tooFewSegments:
				'Manifest alias "{{alias}}" must have at least 3 dot-separated segments (e.g., "Umb.Type.Name").',
			schemaSegmentCount:
				'propertyEditorSchema alias "{{alias}}" must have 2 or 3 dot-separated segments (e.g., "Umbraco.TextBox" or "Umbraco.ColorPicker.EyeDropper").',
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

		// Try to get TypeScript services (unavailable for .js files or when project is not configured).
		let services;
		let checker;
		try {
			services = ESLintUtils.getParserServices(context);
			checker = services.program?.getTypeChecker();
		} catch {
			// No type info available — fall through to structural detection only.
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

				// Must have an alias property with a string literal value.
				if (!aliasProperty) return;
				if (aliasProperty.value.type !== 'Literal' || typeof aliasProperty.value.value !== 'string') return;

				// Determine if this object is a manifest.
				let isManifest = false;

				if (checker && services) {
					// Type-aware: check if the object's type extends ManifestBase.
					const tsNode = services.esTreeNodeToTSNodeMap.get(node);
					const objectType = checker.getTypeAtLocation(tsNode);
					isManifest = typeExtendsName(objectType, 'ManifestBase', checker);
				}

				if (!isManifest) {
					// Structural fallback: require both `type` and `alias` properties.
					const typeProperty = node.properties.find(
						(p) =>
							p.type === 'Property' &&
							!p.computed &&
							((p.key.type === 'Identifier' && p.key.name === 'type') ||
								(p.key.type === 'Literal' && p.key.value === 'type')),
					);
					if (!typeProperty) return;
					isManifest = true;
				}

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

				// Resolve the manifest type (literal property or TS type).
				const manifestType =
					checker && services ? resolveManifestType(node, checker, services) : undefined;
				const isSchemaType = manifestType === 'propertyEditorSchema';

				// Theme aliases are free-form — no format rules enforced.
				if (manifestType === 'theme') return;

				if (isSchemaType) {
					// propertyEditorSchema aliases must start with "Umbraco." and have 2–3 segments.
					if (segments[0] !== 'Umbraco') {
						context.report({
							node: aliasProperty.value,
							messageId: 'missingUmbracoPrefix',
							data: { alias },
						});
						return;
					}

					if (segments.length < 2 || segments.length > 3) {
						context.report({
							node: aliasProperty.value,
							messageId: 'schemaSegmentCount',
							data: { alias },
						});
						return;
					}
				} else {
					// All other manifests must start with "Umb." and have at least 3 segments.
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
				}

				// Every segment must be PascalCase.
				// For localization manifests, the last segment may be a locale code (e.g., "CS_CZ", "DA-DK").
				const isLocalizationType = manifestType === 'localization';

				for (let i = 0; i < segments.length; i++) {
					const segment = segments[i];
					const isLastSegment = i === segments.length - 1;
					const valid = isLastSegment && isLocalizationType ? isLocaleCode(segment) : isPascalCase(segment);

					if (!valid) {
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
