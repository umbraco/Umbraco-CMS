import { UMB_DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS, UMB_DOCUMENT_TYPE_SEARCH_PROVIDER_ALIAS } from '../../constants.js';
import { UMB_DOCUMENT_TYPE_PICKER_MODAL } from '../../modals/index.js';
import type { UmbDocumentTypePickerModalData, UmbDocumentTypePickerModalValue } from '../../modals/index.js';
import type { UmbDocumentTypeItemModel } from '../../types.js';
import type { UmbDocumentTypeTreeItemModel } from '../../tree/types.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentTypePickerInputContext extends UmbPickerInputContext<
	UmbDocumentTypeItemModel,
	UmbDocumentTypeTreeItemModel,
	UmbDocumentTypePickerModalData,
	UmbDocumentTypePickerModalValue
> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS, UMB_DOCUMENT_TYPE_PICKER_MODAL);
	}

	override async openPicker(
		pickerData?: Partial<UmbDocumentTypePickerModalData>,
		args?: UmbDocumentTypePickerInputContextOpenArgs,
	): Promise<void> {
		const combinedPickerData = {
			...pickerData,
		};

		if (!pickerData?.search) {
			combinedPickerData.search = {
				providerAlias: UMB_DOCUMENT_TYPE_SEARCH_PROVIDER_ALIAS,
				...pickerData?.search,
			};
		}

		await super.openPicker(combinedPickerData);
	}
}
