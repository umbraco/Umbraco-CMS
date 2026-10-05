import type { UmbPotentialContentValueModel } from '../types.js';
import { umbVariantObjectSortCompare } from '@umbraco-cms/backoffice/variant';

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
		const variantCompare = umbVariantObjectSortCompare(a, b);
		if (variantCompare !== 0) return variantCompare;
		if (a.alias === b.alias) return 0;
		return a.alias < b.alias ? -1 : 1;
	});
}
