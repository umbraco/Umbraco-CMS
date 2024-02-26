import { createObservablePart } from '../utils/create-observable-part.function.js';
import { deepFreeze } from '../utils/deep-freeze.function.js';
import type { MappingFunction } from '../types/mapping-function.type.js';
import type { MemoizationFunction } from '../types/memoization-function.type.js';
import { naiveObjectComparison } from '../utils/naive-object-comparison.function.js';
import { UmbBasicState } from './basic-state.js';

/**
 * @export
 * @class UmbDeepState
 * @extends {BehaviorSubject<T>}
 * @description - A RxJS BehaviorSubject which deepFreezes the data to ensure its not manipulated from any implementations.
 * Additionally the Subject ensures the data is unique, not updating any Observes unless there is an actual change of the content.
 */
export class UmbDeepState<T> extends UmbBasicState<T> {
	constructor(initialData: T) {
		super(deepFreeze(initialData));
	}

	/**
	 * @export
	 * @method createObservablePart
	 * @param {(mappable: T) => R} mappingFunction - Method to return the part for this Observable to return.
	 * @param {(previousResult: R, currentResult: R) => boolean} [memoizationFunction] - Method to Compare if the data has changed. Should return true when data is different.
	 * @description - Creates an Observable from this State.
	 */
	asObservablePart<ReturnType>(
		mappingFunction: MappingFunction<T, ReturnType>,
		memoizationFunction?: MemoizationFunction<ReturnType>,
	) {
		return createObservablePart(this._subject, mappingFunction, memoizationFunction ?? naiveObjectComparison);
	}

	/**
	 * @method setValue
	 * @param {T} data - The next data for this state to hold.
	 * @description - Set the data of this state, if data is different than current this will trigger observations to update.
	 */
	setValue(data: T): void {
		if (!this._subject) return;
		const frozenData = deepFreeze(data);
		// Only update data if its different than current data.
		if (!naiveObjectComparison(frozenData, this._subject.getValue())) {
			this._subject.next(frozenData);
		}
	}
}
