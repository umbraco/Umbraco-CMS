import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentPickerSearchResultItemContext extends UmbContextBase<UmbDocumentPickerSearchResultItemContext> {
	constructor(host: UmbControllerHost) {
		super(host, 'pickerSearchResultItem');
	}
}

export { UmbDocumentPickerSearchResultItemContext as api };
