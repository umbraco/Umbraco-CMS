import type { UmbDocumentPickerModalData, UmbDocumentPickerModalValue } from '../../modals/types.js';
import {
	UMB_DOCUMENT_PICKER_MODAL,
	UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS,
	UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS,
} from '../../constants.js';
import type { UmbDocumentItemModel } from '../../repository/index.js';
import type { UmbDocumentTreeItemModel } from '../../tree/types.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDocumentTypeEntityType } from '@umbraco-cms/backoffice/document-type';

interface UmbOpenDocumentPickerArgs {
	allowedContentTypes?: Array<{ unique: string; entityType: UmbDocumentTypeEntityType }>;
}

export class UmbDocumentPickerInputContext extends UmbPickerInputContext<
	UmbDocumentItemModel,
	UmbDocumentTreeItemModel,
	UmbDocumentPickerModalData,
	UmbDocumentPickerModalValue
> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS, UMB_DOCUMENT_PICKER_MODAL, (entry) => entry.unique);
	}

	override async openPicker(pickerData?: Partial<UmbDocumentPickerModalData>, args?: UmbOpenDocumentPickerArgs) {
		const pickerConfig = {
			...pickerData,
		};

		pickerConfig.pickableFilter = (item) => this.#pickableFilter(item, args?.allowedContentTypes);

		if (!pickerData?.search) {
			pickerConfig.search = {
				providerAlias: UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS,
				...pickerData?.search,
			};
		}

		pickerConfig.search!.requestArgs = {
			allowedContentTypes: args?.allowedContentTypes,
			...pickerData?.search?.requestArgs,
		};

		super.openPicker(pickerConfig);
	}

	#pickableFilter = (
		item: UmbDocumentItemModel,
		allowedContentTypes?: Array<{ unique: string; entityType: UmbDocumentTypeEntityType }>,
	): boolean => {
		if (allowedContentTypes && allowedContentTypes.length > 0) {
			return allowedContentTypes
				.map((contentTypeReference) => contentTypeReference.unique)
				.includes(item.documentType.unique);
		}
		return true;
	};
}

/** @deprecated Use `UmbDocumentPickerInputContext` instead. This method will be removed in Umbraco 15. */
export { UmbDocumentPickerInputContext as UmbDocumentPickerContext };
