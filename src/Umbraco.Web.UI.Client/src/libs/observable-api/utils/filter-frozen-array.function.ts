/**
 * @function filterFrozenArray
 * @param {Array<T>} data - RxJS Subject to use for this Observable.
 * @param {(entry: T) => boolean} filterMethod - Method to filter the array.
 * @description - Creates a RxJS Observable from RxJS Subject.
 * @example <caption>Example remove an entry of a ArrayState or a part of DeepState/ObjectState it which is an array. Where the key is unique and the item will be updated if matched with existing.</caption>
 * const newDataSet = filterFrozenArray(myState.getValue(), x => x.id !== "myKey");
 * myState.setValue(newDataSet);
 */
export function filterFrozenArray<T>(data: T[], filterMethod: (entry: T) => boolean): T[] {
	return [...data].filter((x) => filterMethod(x));
}
