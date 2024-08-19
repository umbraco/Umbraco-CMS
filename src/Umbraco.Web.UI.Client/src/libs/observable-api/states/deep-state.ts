import { createObservablePart } from '../utils/create-observable-part.function.js';
import { deepFreeze } from '../utils/deep-freeze.function.js';
import type { MappingFunction } from '../types/mapping-function.type.js';
import type { MemoizationFunction } from '../types/memoization-function.type.js';
import { jsonStringComparison } from '../utils/json-string-comparison.function.js';
import { UmbBasicState } from './basic-state.js';

/**
 * @class UmbDeepState
 * @augments {BehaviorSubject<T>}
 * @description - A RxJS BehaviorSubject which deepFreezes the data to ensure its not manipulated from any implementations.
 * Additionally the Subject ensures the data is unique, not updating any Observes unless there is an actual change of the content.
 */
export class UmbDeepState<T> extends UmbBasicState<T> {
	#mute?: boolean;
	#value: T;

	constructor(initialData: T) {
		super(deepFreeze(initialData));
		this.#value = this._subject.getValue();
	}

	/**
	 * @function createObservablePart
	 * @param {(mappable: T) => R} mappingFunction - Method to return the part for this Observable to return.
	 * @param {(previousResult: R, currentResult: R) => boolean} [memoizationFunction] - Method to Compare if the data has changed. Should return true when data is different.
	 * @returns {Observable<R>}
	 * @description - Creates an Observable from this State.
	 */
	asObservablePart<ReturnType>(
		mappingFunction: MappingFunction<T, ReturnType>,
		memoizationFunction?: MemoizationFunction<ReturnType>,
	) {
		return createObservablePart(this._subject, mappingFunction, memoizationFunction ?? jsonStringComparison);
	}

	/**
	 * @function setValue
	 * @param {T} data - The next data for this state to hold.
	 * @description - Set the data of this state, if data is different than current this will trigger observations to update.
	 */
	override setValue(data: T): void {
		if (!this._subject) return;
		const frozenData = deepFreeze(data);
		this.#value = frozenData;
		// Only update data if its not muted and is different than current data. [NL]
		if (!this.#mute && !jsonStringComparison(frozenData, this._subject.getValue())) {
			this._subject.next(frozenData);
		}
	}

	override getValue(): T {
		return this.#value;
	}

	/**
	 * @function mute
	 * @description - Set mute for this state.
	 */
	mute() {
		if (this.#mute) return;
		this.#mute = true;
	}

	/**
	 * @function unmute
	 * @description - Unset the mute of this state.
	 */
	unmute() {
		if (!this.#mute) return;
		this.#mute = false;
		// Only update data if it is different than current data. [NL]
		if (!jsonStringComparison(this.#value, this._subject.getValue())) {
			this._subject?.next(this.#value);
		}
	}

	/**
	 * @function isMuted
	 * @description - Check if the state is muted.
	 * @returns {boolean} - Returns true if the state is muted.
	 */
	isMuted() {
		return this.#mute;
	}

	/**
	 * @function getMutePromise
	 * @description - Get a promise which resolves when the mute is unset.
	 * @returns {Promise<void>}
	 */
	getMutePromise() {
		return new Promise<void>((resolve) => {
			if (!this.#mute) {
				resolve();
				return;
			}
			const subscription = this._subject.subscribe(() => {
				subscription.unsubscribe();
				resolve();
			});
		});
	}
}
