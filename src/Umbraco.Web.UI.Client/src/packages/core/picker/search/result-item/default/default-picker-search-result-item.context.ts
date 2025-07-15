import { UMB_PICKER_SEARCH_RESULT_ITEM_CONTEXT } from './default-picker-search-result-item.context.token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDefaultPickerSearchResultItemContext extends UmbContextBase {
	constructor(host: UmbControllerHost) {
		super(host, UMB_PICKER_SEARCH_RESULT_ITEM_CONTEXT);
	}
}

export { UmbDefaultPickerSearchResultItemContext as api };
