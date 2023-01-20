import { BehaviorSubject, distinctUntilChanged, map, Observable, shareReplay } from "rxjs";


// TODO: Should this handle array as well?
function deepFreeze<T>(inObj: T): T {
	if(typeof inObj === 'object') {
		Object.freeze(inObj);

		Object.getOwnPropertyNames(inObj)?.forEach(function (prop) {
			// eslint-disable-next-line no-prototype-builtins
			if ((inObj as any).hasOwnProperty(prop)
				&& (inObj as any)[prop] != null
				&& typeof (inObj as any)[prop] === 'object'
				&& !Object.isFrozen((inObj as any)[prop])) {
					deepFreeze((inObj as any)[prop]);
				}
		});
	}
	return inObj;
}


export function naiveObjectComparison(objOne: any, objTwo: any): boolean {
	return JSON.stringify(objOne) === JSON.stringify(objTwo);
}





/**
 * @export
 * @method appendToFrozenArray
 * @param {Observable<T>} source - RxJS Subject to use for this Observable.
 * @param {(mappable: T) => R} mappingFunction - Method to return the part for this Observable to return.
 * @param {(previousResult: R, currentResult: R) => boolean} [memoizationFunction] - Method to Compare if the data has changed. Should return true when data is different.
 * @description - Creates a RxJS Observable from RxJS Subject.
 * @example <caption>Example append new entry for a UniqueBehaviorSubject which is an array. Where the key is unique and the item will be updated if matched with existing.</caption>
 * const entry = {key: 'myKey', value: 'myValue'};
 * const newDataSet = appendToFrozenArray(mySubject.getValue(), entry, x => x.key === key);
 * mySubject.next(newDataSet);
 */
export function appendToFrozenArray<T>(data: T[], entry: T, uniqueMethod?: (existingEntry: T, newEntry: T) => boolean): T[] {
	const unFrozenDataSet = [...data];
	if(uniqueMethod) {
		const indexToReplace = unFrozenDataSet.findIndex((x) => uniqueMethod(x, entry));
		if(indexToReplace !== -1) {
			unFrozenDataSet[indexToReplace] = entry;
		} else {
			unFrozenDataSet.push(entry);
		}
	} else {
		unFrozenDataSet.push(entry);
	}
	return unFrozenDataSet;
}




type MappingFunction<T, R> = (mappable: T) => R;
type MemoizationFunction<R> = (previousResult: R, currentResult: R) => boolean;

function defaultMemoization(previousValue: any, currentValue: any): boolean {
	if (typeof previousValue === 'object' && typeof currentValue === 'object') {
	return naiveObjectComparison(previousValue, currentValue);
	}
	return previousValue === currentValue;
}

/**
 * @export
 * @method createObservablePart
 * @param {Observable<T>} source - RxJS Subject to use for this Observable.
 * @param {(mappable: T) => R} mappingFunction - Method to return the part for this Observable to return.
 * @param {(previousResult: R, currentResult: R) => boolean} [memoizationFunction] - Method to Compare if the data has changed. Should return true when data is different.
 * @description - Creates a RxJS Observable from RxJS Subject.
 * @example <caption>Example create a Observable for part of the data Subject.</caption>
 * public readonly myPart = CreateObservablePart(this._data, (data) => data.myPart);
 */
export function createObservablePart<T, R> (
	source$: Observable<T>,
		mappingFunction: MappingFunction<T, R>,
		memoizationFunction?: MemoizationFunction<R>
	): Observable<R> {
	return source$.pipe(
		map(mappingFunction),
		distinctUntilChanged(memoizationFunction || defaultMemoization),
		shareReplay(1)
	)
}


/**
 * @export
 * @class UniqueBehaviorSubject
 * @extends {BehaviorSubject<T>}
 * @description - A RxJS BehaviorSubject which deepFreezes the data to ensure its not manipulated from any implementations.
 * Additionally the Subject ensures the data is unique, not updating any Observes unless there is an actual change of the content.
 */
export class UniqueBehaviorSubject<T> extends BehaviorSubject<T> {
	constructor(initialData: T) {
		super(deepFreeze(initialData));
	}

	next(newData: T): void {
		const frozenData = deepFreeze(newData);
		// Only update data if its different than current data.
		if (!naiveObjectComparison(frozenData, this.getValue())) {
			super.next(frozenData);
		}
	}
}
