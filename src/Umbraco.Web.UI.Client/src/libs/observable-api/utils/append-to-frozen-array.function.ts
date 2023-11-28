/**
 * @export
 * @method appendToFrozenArray
 * @param {Observable<T>} source - RxJS Subject to use for this Observable.
 * @param {(mappable: T) => R} mappingFunction - Method to return the part for this Observable to return.
 * @param {(previousResult: R, currentResult: R) => boolean} [memoizationFunction] - Method to Compare if the data has changed. Should return true when data is different.
 * @description - Creates a RxJS Observable from RxJS Subject.
 * @example <caption>Example append new entry for a ArrayState or a part of UmbDeepState/UmbObjectState it which is an array. Where the key is unique and the item will be updated if matched with existing.</caption>
 * const entry = {id: 'myKey', value: 'myValue'};
 * const newDataSet = appendToFrozenArray(mySubject.getValue(), entry, x => x.id === id);
 * mySubject.next(newDataSet);
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
