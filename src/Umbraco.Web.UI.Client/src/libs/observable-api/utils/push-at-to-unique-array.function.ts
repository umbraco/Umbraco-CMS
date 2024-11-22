/**
 * @function pushToUniqueArray
 * @param {T[]} data - An array of objects.
 * @param index
 * @param {T} entry - The object to insert or replace with.
 * @param {getUniqueMethod: (entry: T) => unknown} [getUniqueMethod] - Method to get the unique value of an entry.
 * @description - Append or replaces an item of an Array.
 * @returns {T[]} - Returns a new array with the updated entry.
 * @example <caption>Example append new entry for a Array. Where the key is unique and the item will be updated if matched with existing.</caption>
 * const entry = {key: 'myKey', value: 'myValue'};
 * const newDataSet = pushToUniqueArray([], entry, x => x.key === key, 1);
 * myState.setValue(newDataSet);
 */
export function pushAtToUniqueArray<T>(
	data: T[],
	entry: T,
	getUniqueMethod: (entry: T) => unknown,
	index: number,
): T[] {
	const unique = getUniqueMethod(entry);
	const indexToReplace = data.findIndex((x) => getUniqueMethod(x) === unique);
	if (indexToReplace !== -1) {
		data[indexToReplace] = entry;
	} else if (index === -1 || index >= data.length) {
		data.push(entry);
	} else {
		data.splice(index, 0, entry);
	}
	return data;
}
