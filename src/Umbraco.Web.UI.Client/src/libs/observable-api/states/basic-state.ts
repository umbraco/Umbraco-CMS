import { BehaviorSubject } from '@umbraco-cms/backoffice/external/rxjs';

/**
 * @export
 * @class UmbBasicState
 * @description - State ensures the data is unique, not updating any Observes unless there is an actual change of the value using `===`.
 */
export class UmbBasicState<T> {
	protected _subject: BehaviorSubject<T>;

	constructor(initialData: T) {
		this._subject = new BehaviorSubject(initialData);
	}

	/**
	 * @method asObservable
	 * @return {Observable} Observable that the State casts to.
	 * @description - Creates a new Observable with this State as the source. Observe this to subscribe to its value and future changes.
	 * @example <caption>Example observe the data of a state</caption>
	 * const myState = new UmbArrayState('Hello world');
	 *
	 * this.observe(myState, (latestStateValue) => console.log("Value is: ", latestStateValue));
	 */
	public asObservable(): ReturnType<BehaviorSubject<T>['asObservable']> {
		return this._subject.asObservable();
	}

	/**
	 * @property value
	 * @description - Holds the current data of this state.
	 * @example <caption>Example retrieve the current data of a state</caption>
	 * const myState = new UmbArrayState('Hello world');
	 * console.log("Value is: ", myState.value);
	 */
	public get value(): BehaviorSubject<T>['value'] {
		return this._subject.value;
	}

	/**
	 * @method getValue
	 * @return {T} The current data of this state.
	 * @description - Provides the current data of this state.
	 * @example <caption>Example retrieve the current data of a state</caption>
	 * const myState = new UmbArrayState('Hello world');
	 * console.log("Value is: ", myState.value);
	 */
	public getValue(): ReturnType<BehaviorSubject<T>['getValue']> {
		return this._subject.getValue();
	}

	/**
	 * @method destroy
	 * @description - Destroys this state and completes all observations made to it.
	 */
	public destroy(): void {
		this._subject?.complete();
		(this._subject as any) = undefined;
	}

	/**
	 * @method setValue
	 * @param {T} data - The next data for this state to hold.
	 * @description - Set the data of this state, if data is different than current this will trigger observations to update.
	 * @example <caption>Example change the data of a state</caption>
	 * const myState = new UmbArrayState('Good morning');
	 * // myState.value is equal 'Good morning'.
	 * myState.setValue('Goodnight')
	 * // myState.value is equal 'Goodnight'.
	 */
	setValue(data: T): void {
		if (this._subject && data !== this._subject.getValue()) {
			this._subject.next(data);
		}
	}

	/**
	 * @deprecated - Use `setValue` instead.
	 */
	next = this.setValue;
}
