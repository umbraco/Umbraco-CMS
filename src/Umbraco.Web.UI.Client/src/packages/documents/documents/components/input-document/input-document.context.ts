import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_DOCUMENT_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';
import { DocumentItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbDocumentPickerContext extends UmbPickerInputContext<DocumentItemResponseModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Repository.Document', UMB_DOCUMENT_PICKER_MODAL);
	}
}
