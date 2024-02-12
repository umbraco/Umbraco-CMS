import type { MemoizationFunction } from '../types/memoization-function.type.js';
import type { MappingFunction } from '../types/mapping-function.type.js';
import { defaultMemoization } from './default-memoization.function.js';
import { type Observable, combineLatest } from '@umbraco-cms/backoffice/external/rxjs';
import { distinctUntilChanged, map, shareReplay } from '@umbraco-cms/backoffice/external/rxjs';

type ArrayToObservableTypes<T extends Array<Observable<any>>> = {
	[K in keyof T]: T[K] extends Observable<infer U> ? U : never;
};

/**
 * @export
 * @method combineObservables
 * @param {Array<Observable<T>>} sources - an Array of RxJS Subjects to use for this Observable.
 * @param {(mappable: Array<T>) => R} mappingFunction - Method to return the part for this Observable to return.
 * @param {(previousResult: R, currentResult: R) => boolean} [memoizationFunction] - Method to Compare if the data has changed. Should return true when data is different.
 * @description - Creates a RxJS Observable from two other Observables.
 */

export function combineObservables<
	R,
	SourceTypes extends Array<Observable<unknown>>,
	SourceTypeArray = ArrayToObservableTypes<SourceTypes>,
>(
	sources: [...SourceTypes],
	mappingFunction: MappingFunction<SourceTypeArray, R>,
	memoizationFunction?: MemoizationFunction<R>,
): Observable<R> {
	return combineLatest(sources).pipe(
		map(mappingFunction as MappingFunction<unknown[], R>),
		distinctUntilChanged(memoizationFunction || defaultMemoization),
		shareReplay(1),
	);
}
