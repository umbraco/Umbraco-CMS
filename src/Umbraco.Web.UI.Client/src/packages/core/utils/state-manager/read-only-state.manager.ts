import type { UmbState } from './state.manager.js';
import { UmbStateManager } from './state.manager.js';

export class UmbReadOnlyStateManager<StateType extends UmbState> extends UmbStateManager<StateType> {
	readonly isReadOnly = this.isOn;
}
