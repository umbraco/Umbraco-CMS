import { UMB_MEDIA_PICKER_CONTEXT } from './media-picker.context.token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbInteractionMemoryManager } from '@umbraco-cms/backoffice/interaction-memory';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

// TODO: extend UmbTreeItemPickerContext
export class UmbMediaPickerContext extends UmbContextBase {
	// For context token safety:
	public readonly IS_MEDIA_PICKER_CONTEXT = true;

	public readonly interactionMemory = new UmbInteractionMemoryManager(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_PICKER_CONTEXT);
	}
}

export { UmbMediaPickerContext as api };
