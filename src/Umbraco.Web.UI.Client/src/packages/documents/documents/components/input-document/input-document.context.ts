import type { UmbDocumentPickerModalData, UmbDocumentPickerModalValue } from '../../modals/types.js';
import { UMB_DOCUMENT_PICKER_MODAL, UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS } from '../../constants.js';
import type { UmbDocumentItemModel } from '../../repository/index.js';
import type { UmbDocumentTreeItemModel } from '../../tree/types.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentPickerInputContext extends UmbPickerInputContext<
	UmbDocumentItemModel,
	UmbDocumentTreeItemModel,
	UmbDocumentPickerModalData,
	UmbDocumentPickerModalValue
> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS, UMB_DOCUMENT_PICKER_MODAL, (entry) => entry.unique);
	}
}

/** @deprecated Use `UmbDocumentPickerInputContext` instead. This method will be removed in Umbraco 15. */
export { UmbDocumentPickerInputContext as UmbDocumentPickerContext };
