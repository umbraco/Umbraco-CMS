/**
 * @function prependToUniqueArray
 * @param {T[]} data - An array of objects.
 * @param {T} entry - The object to insert or replace with.
 * @param {getUniqueMethod: (entry: T) => unknown} [getUniqueMethod] - Method to get the unique value of an entry.
 * @description - Prepend or replaces an item of an Array.
 * @returns {T[]} - The new array with the entry prepended or replaced.
 * @example <caption>Example prepend new entry for a Array. Where the key is unique and the item will be updated if matched with existing.</caption>
 * const entry = {key: 'myKey', value: 'myValue'};
 * const newDataSet = prependToUniqueArray([], entry, x => x.key === key);
 * myState.setValue(newDataSet);
 */
export function prependToUniqueArray<T>(data: T[], entry: T, getUniqueMethod: (entry: T) => unknown): T[] {
	const unique = getUniqueMethod(entry);
	const indexToReplace = data.findIndex((x) => getUniqueMethod(x) === unique);
	if (indexToReplace !== -1) {
		data[indexToReplace] = entry;
	} else {
		data.unshift(entry);
	}
	return data;
}
