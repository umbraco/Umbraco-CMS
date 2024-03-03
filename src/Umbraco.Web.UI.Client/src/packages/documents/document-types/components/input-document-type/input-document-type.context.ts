import type { UmbDocumentTypeItemModel } from '../../repository/index.js';
import { DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_DOCUMENT_TYPE_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';

export class UmbDocumentTypePickerContext extends UmbPickerInputContext<UmbDocumentTypeItemModel> {
	constructor(host: UmbControllerHost) {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		super(host, DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS, UMB_DOCUMENT_TYPE_PICKER_MODAL);
	}
}
