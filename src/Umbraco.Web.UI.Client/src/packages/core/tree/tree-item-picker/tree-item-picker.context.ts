import { UmbTreeItemPickerExpansionManager } from './tree-item-picker-expansion.manager.js';
import { UmbPickerContext } from '@umbraco-cms/backoffice/picker';

export class UmbTreeItemPickerContext extends UmbPickerContext {
	public readonly expansion = new UmbTreeItemPickerExpansionManager(this, {
		interactionMemoryManager: this.interactionMemory,
	});
}

export { UmbTreeItemPickerContext as api };
