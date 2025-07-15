import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

export interface UmbState {
	unique: string;
	message?: string;
}

export class UmbStateManager<StateType extends UmbState = UmbState> extends UmbControllerBase {
	/**
	 * Observable that emits all states in the state manager
	 * @memberof UmbStateManager
	 */
	protected _states = new UmbArrayState<StateType>([], (x) => x.unique);
	public states = this._states.asObservable();

	/**
	 * Observable that emits true if there are any states in the state manager
	 * @memberof UmbStateManager
	 */
	public isOn = this._states.asObservablePart((x) => x.length > 0);

	/**
	 * Observable that emits true if there are no states in the state manager
	 * @memberof UmbStateManager
	 */
	public isOff = this._states.asObservablePart((x) => x.length === 0);

	/**
	 * Creates an instance of UmbStateManager.
	 * @param {UmbControllerHost} host
	 * @memberof UmbStateManager
	 */
	constructor(host: UmbControllerHost) {
		super(host);
	}

	/**
	 * Add a new state to the state manager
	 * @param {StateType} state
	 * @memberof UmbStateManager
	 */
	addState(state: StateType) {
		if (!state.unique) throw new Error('State must have a unique property');
		if (this._states.getValue().find((x) => x.unique === state.unique)) {
			throw new Error('State with unique already exists');
		}
		this._states.setValue([...this._states.getValue(), state]);
	}

	/**
	 * Add multiple states to the state manager
	 * @param {StateType[]} states
	 * @memberof UmbStateManager
	 */
	addStates(states: StateType[]) {
		states.forEach((state) => this.addState(state));
	}

	/**
	 * Remove a state from the state manager
	 * @param {StateType['unique']} unique
	 * @memberof UmbStateManager
	 */
	removeState(unique: StateType['unique']) {
		this._states.removeOne(unique);
	}

	/**
	 * Remove multiple states from the state manager
	 * @param {StateType['unique'][]} uniques
	 * @memberof UmbStateManager
	 */
	removeStates(uniques: StateType['unique'][]) {
		this._states.remove(uniques);
	}

	/**
	 * Get all states from the state manager
	 * @returns {StateType[]} {StateType[]} All states in the state manager
	 * @memberof UmbStateManager
	 */
	getStates() {
		return this._states.getValue();
	}

	getIsOn(): boolean {
		return this._states.getValue().length > 0;
	}

	getIsOff(): boolean {
		return this._states.getValue().length === 0;
	}

	/**
	 * Clear all states from the state manager
	 * @memberof UmbStateManager
	 */
	clear() {
		this._states.setValue([]);
	}

	override destroy() {
		super.destroy();
		this._states.destroy();
	}
}
