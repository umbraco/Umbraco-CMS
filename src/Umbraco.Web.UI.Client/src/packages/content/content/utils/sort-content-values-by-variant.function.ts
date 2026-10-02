import type { UmbPotentialContentValueModel } from '../types.js';

/**
 * Compares two culture or segment codes, invariant (nullish) first, then ordinally, case-sensitively —
 * matching .NET's `StringComparer.Ordinal`, which the Management API uses to order property values for
 * serialization.
 * @param {string | null | undefined} a - The first code to compare.
 * @param {string | null | undefined} b - The second code to compare.
 * @returns {number} A negative number if `a` sorts before `b`, positive if after, zero if equal.
 */
function compareVariantPart(a: string | null | undefined, b: string | null | undefined): number {
	const normalizedA = a ?? null;
	const normalizedB = b ?? null;
	if (normalizedA === normalizedB) return 0;
	if (normalizedA === null) return -1;
	if (normalizedB === null) return 1;
	return normalizedA < normalizedB ? -1 : 1;
}

/**
 * Sorts content values by culture, then by segment, then by alias, matching the ordinal order the
 * Management API applies when serializing property values (invariant culture/segment first, then codes
 * and aliases compared ordinally, case-sensitively). This keeps a freshly-constructed value array in the
 * same order the backend returns after a save and reload, so unchanged values don't register as changed.
 * @param {Array<UmbPotentialContentValueModel>} values - The values to sort.
 * @returns {Array<UmbPotentialContentValueModel>} A new, variant-sorted array.
 */
export function sortContentValuesByVariant<T extends UmbPotentialContentValueModel>(values: Array<T>): Array<T> {
	return [...values].sort((a, b) => {
		const cultureCompare = compareVariantPart(a.culture, b.culture);
		if (cultureCompare !== 0) return cultureCompare;
		const segmentCompare = compareVariantPart(a.segment, b.segment);
		if (segmentCompare !== 0) return segmentCompare;
		if (a.alias === b.alias) return 0;
		return a.alias < b.alias ? -1 : 1;
	});
}
