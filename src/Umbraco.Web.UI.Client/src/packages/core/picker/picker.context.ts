import { UMB_PICKER_CONTEXT } from './picker.context.token.js';
import { UmbPickerSearchManager } from './search/manager/picker-search.manager.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';

export class UmbPickerContext extends UmbContextBase<UmbPickerContext> {
	public readonly selection = new UmbSelectionManager(this);
	public readonly search = new UmbPickerSearchManager(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_PICKER_CONTEXT);
	}
}
