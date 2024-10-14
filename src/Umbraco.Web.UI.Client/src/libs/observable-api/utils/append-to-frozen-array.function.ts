/**
 * @function appendToFrozenArray
 * @param {Array<T>} source - An Array which is frozen and should be updated.
 * @param data
 * @param {T} entry - A new entry to append to the array.
 * @param {(entry: T) => unknown} getUniqueMethod - Method to retrieve a value of an entry that is unique to it. This enables the method to replace existing value if it matches the unique value.
 * @returns {Array<T>} - Returns a new array with the new entry appended.
 * @description - Inserts or replaces an entry in a frozen array and returns a new array.
 * @example <caption>Example append new entry for a UmbArrayState or a part of UmbObjectState/UmbDeepState which is an array. Where the key is unique and the item will be updated if matched with existing.</caption>
 * const entry = {id: 'myKey', value: 'myValue'};
 * const newDataSet = appendToFrozenArray(myState.getValue(), entry, x => x.id === id);
 * myState.setValue(newDataSet);
 */
export function appendToFrozenArray<T>(data: T[], entry: T, getUniqueMethod?: (entry: T) => unknown): T[] {
	const unFrozenDataSet = [...data];
	if (getUniqueMethod) {
		const unique = getUniqueMethod(entry);
		const indexToReplace = unFrozenDataSet.findIndex((x) => getUniqueMethod(x) === unique);
		if (indexToReplace !== -1) {
			unFrozenDataSet[indexToReplace] = entry;
		} else {
			unFrozenDataSet.push(entry);
		}
	} else {
		unFrozenDataSet.push(entry);
	}
	return unFrozenDataSet;
}
