import type { MappingFunction } from '../types/mapping-function.type.js';
import type { MemoizationFunction } from '../types/memoization-function.type.js';
import type { UmbClassStateData } from '../utils/class-equal-memoization.function.js';
import { createObservablePart } from '../utils/create-observable-part.function.js';
import { UmbBasicState } from './basic-state.js';

/**
 * @class UmbClassState
 * @augments {UmbBasicState<T>}
 * @description - This state can hold class instance which has a equal method to compare in coming instances for changes.
 */
export class UmbClassState<T extends UmbClassStateData | undefined> extends UmbBasicState<T> {
	constructor(initialData: T) {
		super(initialData);
	}

	/**
	 * @function createObservablePart
	 * @param {(mappable: UmbClassStateData | undefined) => unknown} mappingFunction - Method to return the part for this Observable to return.
	 * @param {(previousResult: unknown, currentResult: unknown) => boolean} [memoizationFunction] - Method to Compare if the data has changed. Should return true when data is different.
	 * @returns {Observable<unknown>} - an observable.
	 * @description - Creates an Observable from this State.
	 */
	asObservablePart<ReturnType>(
		mappingFunction: MappingFunction<T, ReturnType>,
		memoizationFunction?: MemoizationFunction<ReturnType>,
	) {
		return createObservablePart(this._subject, mappingFunction, memoizationFunction);
	}

	/**
	 * @function setValue
	 * @param {UmbClassStateData | undefined} data - The next data for this state to hold.
	 * @description - Set the data of this state, if data is different than current this will trigger observations to update.
	 */
	override setValue(data: T): void {
		if (!this._subject) return;
		const oldValue = this._subject.getValue();

		if (data && oldValue?.equal(data)) return;
		this._subject.next(data);
	}
}
