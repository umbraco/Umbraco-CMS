import type { UmbPotentialContentValueModel } from '../types.js';
import { _sortContentValuesByVariant } from './sort-content-values-by-variant.function.js';

/**
 * Inserts or replaces a content value in a frozen array of content values.
 *
 * A replaced entry keeps the array's existing order — it already has a variant-sorted position from when it
 * was first added. A newly inserted entry is placed by culture, segment, and alias, matching the order the
 * backend settles a set of variant values into, so a fresh addition doesn't just land at the end out of order.
 * @template T
 * @param {Array<T>} data - An array of content values, which is frozen and should be updated.
 * @param {T} entry - A new or updated content value.
 * @param {(entry: T) => unknown} getUniqueMethod - Method to retrieve the value that uniquely identifies an entry, used to find an existing match to replace.
 * @returns {Array<T>} A new array with the entry inserted or replacing its match.
 */
export function umbAppendContentValue<T extends UmbPotentialContentValueModel>(
	data: Array<T>,
	entry: T,
	getUniqueMethod: (entry: T) => unknown,
): Array<T> {
	const unique = getUniqueMethod(entry);
	const indexToReplace = data.findIndex((x) => getUniqueMethod(x) === unique);

	if (indexToReplace === -1) {
		return _sortContentValuesByVariant([...data, entry]);
	}

	const values = [...data];
	values[indexToReplace] = entry;
	return values;
}
