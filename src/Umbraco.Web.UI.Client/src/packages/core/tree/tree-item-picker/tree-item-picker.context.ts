import { UmbTreeItemPickerExpansionManager } from './tree-item-picker-expansion.manager.js';
import { UmbTreeItemPickerLocationManager } from './tree-item-picker-location.manager.js';
import { UmbPickerContext } from '@umbraco-cms/backoffice/picker';

export class UmbTreeItemPickerContext extends UmbPickerContext {
	public readonly expansion = new UmbTreeItemPickerExpansionManager(this, {
		interactionMemoryManager: this.interactionMemory,
	});

	public readonly location = new UmbTreeItemPickerLocationManager(this, {
		interactionMemoryManager: this.interactionMemory,
	});
}

export { UmbTreeItemPickerContext as api };
