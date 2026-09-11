import type { UmbEntryValueModel } from '../types.js';
import { sortEntryValuesByCulture } from './sort-entry-values-by-culture.function.js';

/**
 * Inserts or replaces an entry value in a frozen array of entry values.
 *
 * A replaced entry keeps the array's existing order — it already has a culture-sorted position from when it was
 * first added. A newly inserted entry is placed by culture, matching the order the backend settles a set of
 * variant values into, so a fresh addition doesn't just land at the end out of order.
 * @param {Array<T>} data - An array of entry values, which is frozen and should be updated.
 * @param {T} entry - A new or updated entry value.
 * @param {(entry: T) => unknown} getUniqueMethod - Method to retrieve the value that uniquely identifies an entry, used to find an existing match to replace.
 * @returns {Array<T>} A new array with the entry inserted or replacing its match.
 */
export function UmbEntryAppendValue<T extends UmbEntryValueModel>(
	data: Array<T>,
	entry: T,
	getUniqueMethod: (entry: T) => unknown,
): Array<T> {
	const unique = getUniqueMethod(entry);
	const indexToReplace = data.findIndex((x) => getUniqueMethod(x) === unique);

	if (indexToReplace === -1) {
		return sortEntryValuesByCulture([...data, entry]);
	}

	const values = [...data];
	values[indexToReplace] = entry;
	return values;
}
