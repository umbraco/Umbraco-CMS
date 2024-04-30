import { UMB_DOCUMENT_TYPE_PICKER_MODAL } from '../../modals/index.js';
import type { UmbDocumentTypeItemModel } from '../../repository/index.js';
import type { UmbDocumentTypeTreeItemModel } from '../../tree/types.js';
import { UMB_DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentTypePickerContext extends UmbPickerInputContext<
	UmbDocumentTypeItemModel,
	UmbDocumentTypeTreeItemModel
> {
	constructor(host: UmbControllerHost) {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		super(host, UMB_DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS, UMB_DOCUMENT_TYPE_PICKER_MODAL);
	}
}
