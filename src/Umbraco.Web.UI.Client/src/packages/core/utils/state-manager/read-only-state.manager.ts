import type { UmbState } from './state.manager.js';
import { UmbStateManager } from './state.manager.js';

/**
 * A State Manager to manage read-only states.
 * @export
 * @class UmbReadOnlyStateManager
 * @augments {UmbStateManager<StateType>}
 * @template StateType
 */
export class UmbReadOnlyStateManager<StateType extends UmbState> extends UmbStateManager<StateType> {
	/**
	 * Observable that emits true if the state is read-only
	 * @memberof UmbReadOnlyStateManager
	 */
	readonly isReadOnly = this.isOn;

	/**
	 * Checks if the state is read-only
	 * @returns {boolean} - true if the state is read-only
	 * @memberof UmbReadOnlyStateManager
	 */
	getIsReadOnly(): boolean {
		return this.getIsOn();
	}
}
