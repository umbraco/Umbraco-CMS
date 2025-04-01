import type { UmbStateEntry } from './state.manager.js';
import { UmbStateManager } from './state.manager.js';

export class UmbReadOnlyStateManager<StateType extends UmbStateEntry> extends UmbStateManager<StateType> {
	readonly isReadOnly = this.isOn;
}
