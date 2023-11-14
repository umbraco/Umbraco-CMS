import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_DOCUMENT_TYPE_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';
import { DocumentTypeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbDocumentTypePickerContext extends UmbPickerInputContext<DocumentTypeItemResponseModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Repository.DocumentType', UMB_DOCUMENT_TYPE_PICKER_MODAL);
	}
}
