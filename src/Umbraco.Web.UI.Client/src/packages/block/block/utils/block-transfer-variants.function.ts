export interface UmbBlockTransferLanguageAccess {
	hasAccessToAllLanguages: boolean;
	languages: Array<string>;
	hasAccessToInvariantForVariant: boolean;
}

const isCultureAllowed = (culture: string, access: UmbBlockTransferLanguageAccess) =>
	access.hasAccessToAllLanguages || access.languages.includes(culture);

const unique = <T>(values: Array<T>) => [...new Set(values)];

/**
 * Finds the data of a block that the user is not permitted to transfer.
 * @param {object} args - The cultures of the block's values and the user's language access.
 * @param {Array<string | null>} args.valueCultures - The culture of every value that holds data, `null` for shared data.
 * @param {boolean} args.variesByCulture - Whether the content type of the block varies by culture.
 * @param {UmbBlockTransferLanguageAccess} args.access - The language access of the user.
 * @returns {{ cultures: Array<string>; invariant: boolean }} The cultures the user lacks access to, and whether shared data of culture-varying content is out of reach.
 */
export function umbGetBlockedTransferVariants(args: {
	valueCultures: Array<string | null>;
	variesByCulture: boolean;
	access: UmbBlockTransferLanguageAccess;
}): { cultures: Array<string>; invariant: boolean } {
	const cultures = unique(args.valueCultures.filter((culture): culture is string => culture !== null)).filter(
		(culture) => !isCultureAllowed(culture, args.access),
	);

	const hasInvariantData = args.valueCultures.includes(null);
	const invariant = args.variesByCulture && hasInvariantData && !args.access.hasAccessToInvariantForVariant;

	return { cultures, invariant };
}

/**
 * Resolves the variants to create for a transferred block, and which of them to publish.
 * Exposed cultures the user lacks access to carry no data of the block, so they are left out.
 * @param {object} args - The cultures of the block and the user's language access.
 * @param {Array<string | null>} args.valueCultures - The culture of every value that holds data, `null` for shared data.
 * @param {Array<string | null>} args.exposedCultures - The cultures the block is exposed in.
 * @param {boolean} args.variesByCulture - Whether the content type of the block varies by culture.
 * @param {string | null | undefined} args.fallbackCulture - The culture to use when the block has no culture of its own.
 * @param {UmbBlockTransferLanguageAccess} args.access - The language access of the user.
 * @returns {{ cultures: Array<string | null>; culturesToPublish: Array<string | null> }} The cultures to create variants for, and the cultures to publish.
 */
export function umbResolveBlockTransferCultures(args: {
	valueCultures: Array<string | null>;
	exposedCultures: Array<string | null>;
	variesByCulture: boolean;
	fallbackCulture: string | null | undefined;
	access: UmbBlockTransferLanguageAccess;
}): { cultures: Array<string | null>; culturesToPublish: Array<string | null> } {
	if (!args.variesByCulture) {
		return { cultures: [null], culturesToPublish: args.exposedCultures.length > 0 ? [null] : [] };
	}

	const culturesToPublish = unique(
		args.exposedCultures.filter(
			(culture): culture is string => culture !== null && isCultureAllowed(culture, args.access),
		),
	);
	const valueCultures = args.valueCultures.filter((culture): culture is string => culture !== null);
	const cultures = unique([...valueCultures, ...culturesToPublish]);

	if (cultures.length === 0 && args.fallbackCulture) {
		cultures.push(args.fallbackCulture);
	}

	return { cultures, culturesToPublish };
}
