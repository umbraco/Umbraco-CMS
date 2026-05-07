import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
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
	 * @param {(previousResult: unknown, currentResult: unknown) => boolean} [memoizationFunction] - Method to compare two results. Should return true when data is the same (unchanged), preventing unnecessary emissions.
	 * @returns {Observable<unknown>} - an observable.
	 * @description - Creates an Observable from this State.
	 */
	override asObservablePart<ReturnType>(
		mappingFunction: MappingFunction<T, ReturnType>,
		memoizationFunction?: MemoizationFunction<ReturnType>,
	): Observable<ReturnType> {
		return createObservablePart(this._subject, mappingFunction, memoizationFunction);
	}

	/**
	 * @function setValue
	 * @param {UmbClassStateData | undefined} data - The next data for this state to hold.
	 * @description - Set the data of this state, if data is different than current this will trigger observations to update.
	 */
	override setValue(data: T): void {
		if (!this._subject) throw new Error('_subject is undefined');
		const oldValue = this._subject.getValue();

		// Do a strict compare first, to avoid reacting on the same instance or going from undefined to undefined.
		if (data === oldValue) return;
		// Then if the current value has an equal method, use it to compare with the incoming value.
		if (data && oldValue?.equal(data)) return;
		this._subject.next(data);
	}
}
