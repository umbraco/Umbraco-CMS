import type { UmbPotentialContentValueModel } from '../types.js';

/**
 * Compares two culture or segment codes, invariant (nullish) first, then ordinally, case-insensitively.
 * @param {string | null | undefined} a - The first code to compare.
 * @param {string | null | undefined} b - The second code to compare.
 * @returns {number} A negative number if `a` sorts before `b`, positive if after, zero if equal.
 */
function compareVariantPart(a: string | null | undefined, b: string | null | undefined): number {
	if (a === b) return 0;
	if (!a) return -1;
	if (!b) return 1;
	return a.toLowerCase() < b.toLowerCase() ? -1 : 1;
}

/**
 * Sorts content values by culture, then by segment, matching the order the backend settles a set of
 * variant values into (invariant first, then codes ordinally, case-insensitively). Values sharing the
 * same culture and segment keep their existing relative order.
 * @param {Array<UmbPotentialContentValueModel>} values - The values to sort.
 * @returns {Array<UmbPotentialContentValueModel>} A new, variant-sorted array.
 */
export function sortContentValuesByVariant<T extends UmbPotentialContentValueModel>(values: Array<T>): Array<T> {
	return [...values].sort((a, b) => {
		const cultureCompare = compareVariantPart(a.culture, b.culture);
		if (cultureCompare !== 0) return cultureCompare;
		return compareVariantPart(a.segment, b.segment);
	});
}
