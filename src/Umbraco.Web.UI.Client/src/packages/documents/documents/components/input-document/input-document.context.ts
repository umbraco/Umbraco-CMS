import { UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import type { UmbDocumentItemModel } from '../../repository/index.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_DOCUMENT_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';

export class UmbDocumentPickerContext extends UmbPickerInputContext<UmbDocumentItemModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS, UMB_DOCUMENT_PICKER_MODAL, (entry) => entry.unique);
	}
}
