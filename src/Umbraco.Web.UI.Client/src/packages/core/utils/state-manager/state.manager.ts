import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

export interface UmbState {
	unique: string;
	message: string;
}

export class UmbStateManager<StateType extends UmbState = UmbState> extends UmbControllerBase {
	protected _states = new UmbArrayState<StateType>([], (x) => x.unique);
	public states = this._states.asObservable();

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
		this._states.setValue(this._states.getValue().filter((x) => x.unique !== unique));
	}

	/**
	 * Remove multiple states from the state manager
	 * @param {StateType['unique'][]} uniques
	 * @memberof UmbStateManager
	 */
	removeStates(uniques: StateType['unique'][]) {
		this._states.setValue(this._states.getValue().filter((x) => !uniques.includes(x.unique)));
	}

	/**
	 * Clear all states from the state manager
	 * @memberof UmbStateManager
	 */
	clear() {
		this._states.setValue([]);
	}
}
