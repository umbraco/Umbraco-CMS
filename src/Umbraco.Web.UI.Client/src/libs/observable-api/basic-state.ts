import { BehaviorSubject } from '@umbraco-cms/backoffice/external/rxjs';

/**
 * @export
 * @class UmbBasicState
 * @description - State ensures the data is unique, not updating any Observes unless there is an actual change of the value using `===`.
 */
export class UmbBasicState<T> {

	protected _subject:BehaviorSubject<T>;

	constructor(initialData: T) {
		this._subject = new BehaviorSubject(initialData);
		this.asObservable = this._subject.asObservable.bind(this._subject);
		this.getValue = this._subject.getValue.bind(this._subject);
		this.destroy = this._subject.complete.bind(this._subject);
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
	public asObservable: BehaviorSubject<T>['asObservable'];

	/**
	 * @property value
	 * @description - Holds the current data of this state.
	 * @example <caption>Example retrieve the current data of a state</caption>
	 * const myState = new UmbArrayState('Hello world');
	 * console.log("Value is: ", myState.getValue());
   */
	public get value(): BehaviorSubject<T>['value'] {
		return this._subject.value;
	};

	/**
	 * @method getValue
	 * @return {T} The current data of this state.
	 * @description - Provides the current data of this state.
	 * @example <caption>Example retrieve the current data of a state</caption>
	 * const myState = new UmbArrayState('Hello world');
	 * console.log("Value is: ", myState.value);
   */
	public getValue: BehaviorSubject<T>['getValue'];

	/**
	 * @method destroy
	 * @description - Destroys this state and completes all observations made to it.
   */
	public destroy: BehaviorSubject<T>['complete'];

	/**
	 * @method next
	 * @param {T} data - The next set of data for this state to hold.
	 * @description - Set the data of this state, if data is different than current this will trigger observations to update.
	 * @example <caption>Example retrieve the current data of a state</caption>
	 * const myState = new UmbArrayState('Good morning');
	 * // myState.value is equal 'Good morning'.
	 * myState.next('Goodnight')
	 * // myState.value is equal 'Goodnight'.
   */
	next(newData: T): void {
		if (newData !== this._subject.getValue()) {
			this._subject.next(newData);
		}
	}
}
