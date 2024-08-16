import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDefaultPickerSearchResultItemContext extends UmbContextBase<UmbDefaultPickerSearchResultItemContext> {
	constructor(host: UmbControllerHost) {
		super(host, 'pickerSearchResultItem');
	}
}

export { UmbDefaultPickerSearchResultItemContext as api };
