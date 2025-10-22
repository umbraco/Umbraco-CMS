/**
 * @function replaceInUniqueArray
 * @param {T[]} data - An array of objects.
 * @param {T} entry - The object to replace with.
 * @param {getUniqueMethod: (entry: T) => unknown} [getUniqueMethod] - Method to get the unique value of an entry.
 * @description - Replaces an item of an Array.
 * @example <caption>Example replace an entry of an Array. Where the key is unique and the item will only be replaced if matched with existing.</caption>
 * const data = [{key: 'myKey', value:'initialValue'}];
 * const entry = {key: 'myKey', value: 'replacedValue'};
 * const newDataSet = replaceUniqueArray(data, entry, x => x.key === key);
 */
export function replaceInUniqueArray<T>(data: T[], entry: T, getUniqueMethod: (entry: T) => unknown): T[] {
	const unique = getUniqueMethod(entry);
	const indexToReplace = data.findIndex((x) => getUniqueMethod(x) === unique);
	if (indexToReplace !== -1) {
		data[indexToReplace] = entry;
	}
	return data;
}
