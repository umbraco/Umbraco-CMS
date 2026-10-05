import type { UmbBlockExposeModel } from '../types.js';
import { umbVariantObjectSortCompare } from '@umbraco-cms/backoffice/variant';

/**
 * Compares two block expose entries by culture, segment, then content key, matching the order the
 * Management API applies when serializing block exposes. Content keys are compared ordinally, which equals
 * the order of `Guid.CompareTo` as long as both sides use lowercase hexadecimal keys.
 * @param {UmbBlockExposeModel} a - The first expose to compare.
 * @param {UmbBlockExposeModel} b - The second expose to compare.
 * @returns {number} A negative number if `a` sorts before `b`, positive if after, zero if equal.
 */
export function umbBlockExposeSortCompare(a: UmbBlockExposeModel, b: UmbBlockExposeModel): number {
	const variantCompare = umbVariantObjectSortCompare(a, b);
	if (variantCompare !== 0) return variantCompare;
	if (a.contentKey === b.contentKey) return 0;
	return a.contentKey < b.contentKey ? -1 : 1;
}
