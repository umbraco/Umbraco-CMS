'use strict';

const fs = require('node:fs');
const path = require('node:path');

/**
 * Flags string-literal arguments to `<x>.localize.term(...)` / `<x>.localize.termOrDefault(...)` that
 * aren't in the generated `UMB_KNOWN_LOCALIZATION_KEYS` list (sourced from `assets/lang/en.ts`).
 *
 * Why a lint rule and not just type-checking: the TypeScript signature on `term()` keeps a
 * `(string & {})` escape hatch in the key parameter so dynamic keys (e.g. `` `login_greeting${day}` ``)
 * and third-party-registered runtime keys still type-check without casts. That's necessary for the
 * published `@umbraco-cms/backoffice` types but it means typos in literal keys also slip through tsc.
 * This rule closes the gap inside our own codebase without changing the shipped types.
 *
 * Skips template literals, identifiers, and other non-literal arguments — the escape hatch is
 * intentional for those. To bypass the rule on a specific known-runtime-registered key, use
 * `// eslint-disable-next-line local-rules/no-unknown-localization-key`.
 */

const GENERATED_KEYS_FILE = path.join(__dirname, '../../../src/libs/localization-api/known-keys.generated.ts');

let cachedKeySet = null;

function loadKnownKeys() {
	if (cachedKeySet) return cachedKeySet;
	let source;
	try {
		source = fs.readFileSync(GENERATED_KEYS_FILE, 'utf-8');
	} catch {
		// Generated file missing — fail open so a fresh checkout that hasn't run `npm install`
		// (which triggers `prebuild`/`generate:localization-keys`) doesn't get drowned in errors.
		// CI's `npm run lint:errors` runs after the generator, so this branch only hits locally.
		return new Set();
	}
	const arrayMatch = source.match(/UMB_KNOWN_LOCALIZATION_KEYS[^=]*=\s*\[([\s\S]*?)\];/);
	if (!arrayMatch) {
		throw new Error(
			`Could not locate UMB_KNOWN_LOCALIZATION_KEYS in ${GENERATED_KEYS_FILE}. Regenerate via \`npm run generate:localization-keys\`.`,
		);
	}
	const keys = new Set();
	for (const match of arrayMatch[1].matchAll(/(['"])((?:(?!\1).)+)\1/g)) {
		keys.add(match[2]);
	}
	cachedKeySet = keys;
	return keys;
}

const TARGET_METHODS = new Set(['term', 'termOrDefault']);

/**
 * Returns true when `node` is a `<something>.localize` member access.
 * Matches `this.localize`, `this.#localize`, `host._localize`, etc. — same heuristic as no-unsafe-localize.
 */
function isLocalizeReceiver(node) {
	if (!node || node.type !== 'MemberExpression') return false;
	const prop = node.property;
	if (!prop) return false;
	if (prop.type === 'Identifier' && /localize$/i.test(prop.name)) return true;
	if (prop.type === 'PrivateIdentifier' && /localize$/i.test(prop.name)) return true;
	return false;
}

/** @type {import('eslint').Rule.RuleModule} */
module.exports = {
	meta: {
		type: 'problem',
		docs: {
			description:
				'Disallow passing string literals to `localize.term()` / `localize.termOrDefault()` that are not in the generated `UMB_KNOWN_LOCALIZATION_KEYS` list.',
			category: 'Possible Errors',
			recommended: true,
		},
		schema: [],
		messages: {
			unknownKey:
				"Unknown localization key '{{key}}'. Add it to `src/assets/lang/en.ts` and regenerate (`npm run generate:localization-keys`), or use a dynamic-key form (template literal / variable) for runtime-registered keys.",
		},
	},
	create(context) {
		const knownKeys = loadKnownKeys();
		if (knownKeys.size === 0) return {};

		return {
			CallExpression(node) {
				const callee = node.callee;
				if (!callee || callee.type !== 'MemberExpression') return;

				const methodName = callee.property?.name;
				if (!methodName || !TARGET_METHODS.has(methodName)) return;

				if (!isLocalizeReceiver(callee.object)) return;

				const firstArg = node.arguments[0];
				if (!firstArg) return;
				// Only check static literal keys. Template literals, identifiers, member accesses,
				// and other dynamic forms are the documented escape hatch — leave them alone.
				if (firstArg.type !== 'Literal') return;
				if (typeof firstArg.value !== 'string') return;

				if (knownKeys.has(firstArg.value)) return;

				context.report({
					node: firstArg,
					messageId: 'unknownKey',
					data: { key: firstArg.value },
				});
			},
		};
	},
};
