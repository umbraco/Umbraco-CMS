import { UMB_PICKER_CONTEXT } from './picker.context.token.js';
import { UmbPickerSearchManager } from './search/manager/picker-search.manager.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbInteractionMemoryManager } from '@umbraco-cms/backoffice/interaction-memory';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import { UMB_PROPERTY_TYPE_BASED_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/content';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbPickerContext extends UmbContextBase {
	public readonly interactionMemory = new UmbInteractionMemoryManager(this);
	public readonly selection = new UmbSelectionManager(this);
	public readonly search = new UmbPickerSearchManager(this);

	public dataType?: { unique: string };

	constructor(host: UmbControllerHost) {
		super(host, UMB_PICKER_CONTEXT);

		/* TODO: Move this implementation to another place. The generic picker context shouldn't be aware of property and data types.
		It also gives an illegal import of content module */
		this.consumeContext(UMB_PROPERTY_TYPE_BASED_PROPERTY_CONTEXT, (context) => {
			this.observe(context?.dataType, (dataType) => {
				this.dataType = dataType;
			});
		});
	}
}
