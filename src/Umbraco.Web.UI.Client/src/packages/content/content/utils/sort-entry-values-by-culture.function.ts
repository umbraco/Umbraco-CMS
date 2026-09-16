import type { UmbEntryValueModel } from '../types.js';

/**
 * Sorts entry values by culture, matching the order the backend settles a set of variant values into
 * (culture-invariant values first, then culture codes ordinally, case-insensitively). Values sharing a
 * culture keep their existing relative order.
 * @param {Array<UmbEntryValueModel>} values - The values to sort.
 * @returns {Array<UmbEntryValueModel>} A new, culture-sorted array.
 */
export function sortEntryValuesByCulture<T extends UmbEntryValueModel>(values: Array<T>): Array<T> {
	return [...values].sort((a, b) => {
		if (a.culture === b.culture) return 0;
		if (!a.culture) return -1;
		if (!b.culture) return 1;
		return a.culture.toLowerCase() < b.culture.toLowerCase() ? -1 : 1;
	});
}
