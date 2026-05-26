import type { MappingFunction } from '../types/mapping-function.type.js';
import type { MemoizationFunction } from '../types/memoization-function.type.js';
import { createObservablePart } from '../utils/create-observable-part.function.js';
import { BehaviorSubject, type Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { strictEqualityMemoization } from '../utils/strict-equality-memoization.function.js';

/**
 * @class UmbBasicState
 * @description - State ensures the data is unique, not updating any Observes unless there is an actual change of the value using `===`.
 */
export class UmbBasicState<T> {
	// TODO (V19): Change type to BehaviorSubject<T> | undefined — destroy() sets this to undefined via `as unknown` cast, but the type does not reflect it. Fixing the type will make all existing guards (?.  and if checks) correct and remove the need for the cast.
	protected _subject: BehaviorSubject<T>;

	constructor(initialData: T) {
		this._subject = new BehaviorSubject(initialData);
	}

	/**
	 * @function asObservable
	 * @returns {Observable} Observable that the State casts to.
	 * @description - Creates a new Observable with this State as the source. Observe this to subscribe to its value and future changes.
	 * @example <caption>Example observe the data of a state</caption>
	 * const myState = new UmbArrayState('Hello world');
	 *
	 * this.observe(myState, (latestStateValue) => console.log("Value is: ", latestStateValue));
	 */
	public asObservable(): Observable<T> {
		return this._subject.asObservable();
	}

	/**
	 * @function asObservablePart
	 * @param {(mappable: T) => R} mappingFunction - Method to return the part for this Observable to return.
	 * @param {(previousResult: R, currentResult: R) => boolean} [memoizationFunction] - Method to compare if the data has changed. Should return true when data is identical.
	 * @returns {Observable<R>}
	 * @description - Creates an Observable from this State that emits a derived value, deduplicated against its previous emission.
	 */
	public asObservablePart<ReturnType>(
		mappingFunction: MappingFunction<T, ReturnType>,
		memoizationFunction?: MemoizationFunction<ReturnType>,
	): Observable<ReturnType> {
		return createObservablePart(this._subject, mappingFunction, memoizationFunction ?? strictEqualityMemoization);
	}

	/**
	 * @property {unknown} value - the value of the State.
	 * @description - Holds the current data of this state.
	 * @returns {unknown} Observable that
	 * @example <caption>Example retrieve the current data of a state</caption>
	 * const myState = new UmbArrayState('Hello world');
	 * console.log("Value is: ", myState.value);
	 */
	public get value(): T {
		return this.getValue();
	}

	/**
	 * @function getValue
	 * @returns {unknown} The current data of this state.
	 * @description - Provides the current data of this state.
	 * @example <caption>Example retrieve the current data of a state</caption>
	 * const myState = new UmbArrayState('Hello world');
	 * console.log("Value is: ", myState.value);
	 */
	public getValue(): T {
		return this._subject.getValue();
	}

	/**
	 * @function destroy
	 * @description - Destroys this state and completes all observations made to it.
	 */
	public destroy(): void {
		this._subject?.complete();
		(this._subject as unknown) = undefined;
	}

	/**
	 * @function setValue
	 * @param {unknown} data - The next data for this state to hold.
	 * @description - Set the data of this state, if data is different than current this will trigger observations to update.
	 * @example <caption>Example change the data of a state</caption>
	 * const myState = new UmbArrayState('Good morning');
	 * // myState.value is equal 'Good morning'.
	 * myState.setValue('Goodnight')
	 * // myState.value is equal 'Goodnight'.
	 */
	setValue(data: T): void {
		if (!this._subject) throw new Error('_subject is undefined');
		if (data !== this._subject.getValue()) {
			this._subject.next(data);
		}
	}
}
