import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState, createObservablePart } from '@umbraco-cms/backoffice/observable-api';
import type { UmbPartialSome } from '../type';

export interface UmbState {
	unique: string | symbol;
	state?: boolean;
	message?: string;
}

export interface UmbStateEntry {
	unique: string | symbol;
	state: boolean;
	message?: string;
}

const DefaultStateUnique = Symbol();

export class UmbStateManager<
	StateType extends UmbStateEntry = UmbStateEntry,
	IncomingStateType extends UmbState = UmbPartialSome<StateType, 'state'>,
> extends UmbControllerBase {
	//
	protected readonly _states = new UmbArrayState<StateType>([], (x) => x.unique).sortBy((a, b) =>
		a.state === b.state ? 0 : a.state ? 1 : -1,
	);
	protected readonly _statesObservable = this._states.asObservable();

	// only on states for compatibility.
	// TODO: Describe new methods:
	/**
	 * @obsolete stop using states directly, use the new methods.
	 */
	public readonly states = this._states.asObservablePart((x) => x.filter((x) => x.state === true));

	//public readonly hasAnyStates = this._states.asObservablePart((x) => x.length > 0);

	/**
	 * Observable that emits true if there are any states in the state manager
	 * @memberof UmbStateManager
	 * @obsolete stop using isOn, use the new methods.
	 */
	public readonly isOn = createObservablePart(this.states, (x) => x.length > 0);

	/**
	 * Observable that emits true if there are no states in the state manager
	 * @memberof UmbStateManager
	 * @obsolete stop using isOff, use the new methods.
	 */
	public readonly isOff = createObservablePart(this.states, (x) => x.length === 0);

	public fallbackToOff() {
		this._states.appendOne({ unique: DefaultStateUnique, state: false } as StateType);
	}

	public fallbackToOn() {
		this._states.appendOne({ unique: DefaultStateUnique, state: true } as StateType);
	}

	/**
	 * Add a new state to the state manager
	 * @param {StateType} state
	 * @memberof UmbStateManager
	 */
	addState(state: IncomingStateType) {
		if (!state) throw new Error('State must be defined');
		if (!state.unique) throw new Error('State must have a unique property');
		this._states.appendOne({ state: true, ...state } as unknown as StateType);
	}

	/**
	 * Add multiple states to the state manager
	 * @param {StateType[]} states
	 * @memberof UmbStateManager
	 */
	addStates(states: IncomingStateType[]) {
		this._states.mute();
		states.forEach((state) => this.addState(state));
		this._states.unmute();
	}

	/**
	 * Remove a state from the state manager
	 * @param {StateType['unique']} unique Unique value of the state to remove
	 * @memberof UmbStateManager
	 */
	removeState(unique: StateType['unique']) {
		this._states.removeOne(unique);
	}

	/**
	 * Remove multiple states from the state manager
	 * @param {StateType['unique'][]} uniques Array of unique values to remove
	 * @memberof UmbStateManager
	 */
	removeStates(uniques: StateType['unique'][]) {
		this._states.remove(uniques);
	}

	// TODO: Describe new methods:
	/**
	 * @obsolete stop using states directly, use the new methods.
	 * Get all states from the state manager
	 * @returns {StateType[]} {StateType[]} All states in the state manager
	 * @memberof UmbStateManager
	 */
	getStates(): Array<StateType> {
		return this._states.getValue().filter((x) => x.state === true);
	}
	getAllStates(): Array<StateType> {
		return this._states.getValue();
	}

	/**
	 * Clear all states from the state manager
	 * @memberof UmbStateManager
	 */
	clear(): void {
		this._states.setValue([]);
	}

	/**
	 * Get if there are any states in the state manager
	 * @returns {boolean} True if there are any states in the state manager
	 * @memberof UmbStateManager
	 */
	getIsOn(): boolean {
		return this.getStates().length > 0;
	}

	/**
	 * Get if there are no states in the state manager
	 * @returns {boolean} True if there are no states in the state manager
	 * @memberof UmbStateManager
	 */
	getIsOff(): boolean {
		return this.getStates().length === 0;
	}

	override destroy() {
		this._states.destroy();
		super.destroy();
	}
}
