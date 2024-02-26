import type { MappingFunction } from '../types/mapping-function.type.js';
import type { MemoizationFunction } from '../types/memoization-function.type.js';
import { createObservablePart } from '../utils/create-observable-part.function.js';
import { UmbBasicState } from './basic-state.js';

export interface UmbClassStateData {
	equal(otherClass: this | undefined): boolean;
}

/**
 * @export
 * @class UmbClassState
 * @extends {UmbBasicState<T>}
 * @description - This state can hold class instance which has a equal method to compare in coming instances for changes.
 */
export class UmbClassState<T extends UmbClassStateData | undefined> extends UmbBasicState<T> {
	constructor(initialData: T) {
		super(initialData);
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
		return createObservablePart(this._subject, mappingFunction, memoizationFunction);
	}

	/**
	 * @method setValue
	 * @param {T} data - The next data for this state to hold.
	 * @description - Set the data of this state, if data is different than current this will trigger observations to update.
	 */
	setValue(data: T): void {
		if (!this._subject) return;
		const oldValue = this._subject.getValue();

		if (data && oldValue?.equal(data)) return;
		this._subject.next(data);
	}
}
