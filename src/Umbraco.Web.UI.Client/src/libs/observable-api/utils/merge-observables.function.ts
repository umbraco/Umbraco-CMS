import type { MemoizationFunction } from '../types/memoization-function.type.js';
import type { MappingFunction } from '../types/mapping-function.type.js';
import { defaultMemoization } from './default-memoization.function.js';
import { type Observable, combineLatest } from '@umbraco-cms/backoffice/external/rxjs';
import { distinctUntilChanged, map, shareReplay } from '@umbraco-cms/backoffice/external/rxjs';

type ArrayToObservableTypes<T extends Array<Observable<any>>> = {
	[K in keyof T]: T[K] extends Observable<infer U> ? U : never;
};

/**
 * @function mergeObservables
 * @param {Array<Observable<T>>} sources - an Array of Observables to merge for this Observable.
 * @param {(mappable: Array<T>) => R} mergeFunction - Merge method to return the part for this Observable to return.
 * @param {(previousResult: R, currentResult: R) => boolean} [memoizationFunction] - Method to Compare if the merged data has changed. Should return true when data is different.
 * @returns {Observable<R>} - Returns a new Observable that combines the Observables into a single Observable.
 * @description - Creates a Observable from two or more Observables.
 * @example
 *
 * const mergedObservable = mergeObservables(
 * 	[observable1, observable2],
 * 	([value1, value2]) => value1 + value2,
 * );
 *
 * if observable1 has the value of 10 and observable2 has the value of 4, the mergedObservable will return the value of 14.
 * if one of them changes the mergedObservable will return the new value.
 */
export function mergeObservables<
	R,
	SourceTypes extends Array<Observable<unknown>>,
	SourceTypeArray = ArrayToObservableTypes<SourceTypes>,
>(
	sources: [...SourceTypes],
	mergeFunction: MappingFunction<SourceTypeArray, R>,
	memoizationFunction?: MemoizationFunction<R>,
): Observable<R> {
	return combineLatest(sources).pipe(
		map(mergeFunction as MappingFunction<unknown[], R>),
		distinctUntilChanged(memoizationFunction ?? defaultMemoization),
		shareReplay(1),
	);
}
