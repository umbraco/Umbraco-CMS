import { UMB_PICKER_CONTEXT } from './picker.context.token.js';
import { UmbPickerSearchManager } from './search/manager/picker-search.manager.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_PROPERTY_TYPE_BASED_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/content';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';

export class UmbPickerContext extends UmbContextBase {
	public readonly selection = new UmbSelectionManager(this);
	public readonly search = new UmbPickerSearchManager(this);
	public dataType?: { unique: string };

	constructor(host: UmbControllerHost) {
		super(host, UMB_PICKER_CONTEXT);

		this.consumeContext(UMB_PROPERTY_TYPE_BASED_PROPERTY_CONTEXT, (context) => {
			this.observe(context?.dataType, (dataType) => {
				this.dataType = dataType;
			});
		});
	}
}
