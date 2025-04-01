import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState, UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

export interface UmbState {
	unique: string;
	message: string;
	message?: string;
}

export class UmbStateManager<StateType extends UmbState = UmbState> extends UmbControllerBase {
	/**
	 * Observable that emits all states in the state manager
	 * @memberof UmbStateManager
	 */
	protected _states = new UmbArrayState<StateType>([], (x) => x.unique);
	public readonly states = this._states.asObservable();

	protected _isRunning = new UmbBooleanState(true);
	public readonly isRunning = this._isRunning.asObservable();

	/**
	 * Observable that emits true if there are any states in the state manager
	 * @memberof UmbStateManager
	 */
	public readonly isOn = this._states.asObservablePart((x) => x.length > 0);

	/**
	 * Observable that emits true if there are no states in the state manager
	 * @memberof UmbStateManager
	 */
	public readonly isOff = this._states.asObservablePart((x) => x.length === 0);

	/**
	 * Start the state - this will allow the state to be used.
	 */
	public start() {
		this._states.unmute();
		this._isRunning.setValue(true);
	}

	/**
	 * Stop the state - this will prevent the state from being used
	 */
	public stop() {
		this._states.mute();
		this._isRunning.setValue(false);
	}

	/**
	 * Get whether the state is running
	 * @returns {boolean} True if the state is running
	 * @memberof UmbStateManager
	 */
	public getIsRunning(): boolean {
		return this._isRunning.getValue();
	}

	/**
	 * Add a new state to the state manager
	 * @param {StateType} state
	 * @memberof UmbStateManager
	 */
	addState(state: StateType) {
		if (this.getIsRunning() === false) {
			throw new Error('State manager is not running. Call start() before adding states');
		}
		if (!state) throw new Error('State must be defined');
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
		if (this.getIsRunning() === false) {
			throw new Error('State manager is not running. Call start() before adding states');
		}

		states.forEach((state) => this.addState(state));
	}

	/**
	 * Remove a state from the state manager
	 * @param {StateType['unique']} unique Unique value of the state to remove
	 * @memberof UmbStateManager
	 */
	removeState(unique: StateType['unique']) {
		this._states.setValue(this._states.getValue().filter((x) => x.unique !== unique));
	}

	/**
	 * Remove multiple states from the state manager
	 * @param {StateType['unique'][]} uniques Array of unique values to remove
	 * @memberof UmbStateManager
	 */
	removeStates(uniques: StateType['unique'][]) {
		this._states.setValue(this._states.getValue().filter((x) => !uniques.includes(x.unique)));
	}

	/**
	 * Get all states from the state manager
	 * @returns {StateType[]} {StateType[]} All states in the state manager
	 * @memberof UmbStateManager
	 */
	getStates() {
		return this._states.getValue();
	}

	/**
	 * Clear all states from the state manager
	 * @memberof UmbStateManager
	 */
	clear() {
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
		this._isRunning.destroy();
		super.destroy();
	}
}
