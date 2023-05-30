import { createObservablePart } from './create-observable-part.function.js';
import { deepFreeze } from './deep-freeze.function.js';
import type { MappingFunction } from './mapping-function.js';
import type { MemoizationFunction } from './memoization-function.js';
import { naiveObjectComparison } from './naive-object-comparison.js';
import { BehaviorSubject } from '@umbraco-cms/backoffice/external/rxjs';

/**
 * @export
 * @class UmbDeepState
 * @extends {BehaviorSubject<T>}
 * @description - A RxJS BehaviorSubject which deepFreezes the data to ensure its not manipulated from any implementations.
 * Additionally the Subject ensures the data is unique, not updating any Observes unless there is an actual change of the content.
 */
export class UmbDeepState<T> extends BehaviorSubject<T> {
	constructor(initialData: T) {
		super(deepFreeze(initialData));
	}

	getObservablePart<ReturnType>(
		mappingFunction: MappingFunction<T, ReturnType>,
		memoizationFunction?: MemoizationFunction<ReturnType>
	) {
		return createObservablePart(this, mappingFunction, memoizationFunction);
	}

	next(newData: T): void {
		const frozenData = deepFreeze(newData);
		// Only update data if its different than current data.
		if (!naiveObjectComparison(frozenData, this.getValue())) {
			super.next(frozenData);
		}
	}
}
