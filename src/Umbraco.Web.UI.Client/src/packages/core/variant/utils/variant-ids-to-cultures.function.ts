import type { UmbVariantId } from '../variant-id.class.js';

/**
 * Maps variant ids to their distinct cultures. Missing variant ids are skipped, so the result never contains `undefined`.
 * The invariant culture is represented as `null` and is kept in the result; filter it out locally where it is not wanted.
 * @param {Array<UmbVariantId | undefined>} variantIds - The variant ids to map.
 * @returns {Array<string | null>} The distinct cultures, in order of first occurrence.
 */
export function umbVariantIdsToCultures(variantIds: Array<UmbVariantId | undefined>): Array<string | null> {
	const cultures = variantIds.filter((x): x is UmbVariantId => x !== undefined).map((x) => x.culture);
	return [...new Set(cultures)];
}
