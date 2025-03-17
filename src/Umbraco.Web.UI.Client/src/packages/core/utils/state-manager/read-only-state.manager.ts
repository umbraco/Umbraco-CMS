import type { UmbState } from './state.manager.js';
import { UmbStateManager } from './state.manager.js';

export class UmbReadOnlyStateManager<StateType extends UmbState> extends UmbStateManager<StateType> {
	readonly isReadOnly = this.isOn;

	/**
	 * Get the read only state
	 * @returns {boolean} True if the state is read only
	 */
	getIsReadOnly(): boolean {
		return this.getIsOn();
	}
}
