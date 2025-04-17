import { UMB_MEDIA_ITEM_REPOSITORY_ALIAS } from '../../constants.js';
import { UMB_MEDIA_PICKER_MODAL } from '../../modals/index.js';
import type { UmbMediaItemModel } from '../../repository/item/types.js';
import type { UmbMediaPickerModalData, UmbMediaPickerModalValue } from '../../modals/index.js';
import { UMB_MEDIA_SEARCH_PROVIDER_ALIAS } from '../../search/constants.js';
import type { UmbMediaTreeItemModel } from '../../tree/types.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbMediaTypeEntityType } from '@umbraco-cms/backoffice/media-type';

interface UmbMediaPickerInputContextOpenArgs {
	allowedContentTypes?: Array<{ unique: string; entityType: UmbMediaTypeEntityType }>;
	includeTrashed?: boolean;
}

export class UmbMediaPickerInputContext extends UmbPickerInputContext<
	UmbMediaItemModel,
	UmbMediaTreeItemModel,
	UmbMediaPickerModalData,
	UmbMediaPickerModalValue
> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_ITEM_REPOSITORY_ALIAS, UMB_MEDIA_PICKER_MODAL);
	}

	override async openPicker(pickerData?: Partial<UmbMediaPickerModalData>, args?: UmbMediaPickerInputContextOpenArgs) {
		const combinedPickerData = {
			...pickerData,
		};

		// transform allowedContentTypes to a pickable filter
		combinedPickerData.pickableFilter = (item) => this.#pickableFilter(item, args?.allowedContentTypes);

		// set default search data
		if (!pickerData?.search) {
			combinedPickerData.search = {
				providerAlias: UMB_MEDIA_SEARCH_PROVIDER_ALIAS,
				...pickerData?.search,
			};
		}

		// pass allowedContentTypes to the search request args
		combinedPickerData.search!.queryParams = {
			allowedContentTypes: args?.allowedContentTypes,
			includeTrashed: args?.includeTrashed,
			...pickerData?.search?.queryParams,
		};

		await super.openPicker(combinedPickerData);
	}

	#pickableFilter = (
		item: UmbMediaItemModel,
		allowedContentTypes?: Array<{ unique: string; entityType: UmbMediaTypeEntityType }>,
	): boolean => {
		if (allowedContentTypes && allowedContentTypes.length > 0) {
			return allowedContentTypes
				.map((contentTypeReference) => contentTypeReference.unique)
				.includes(item.mediaType.unique);
		}
		return true;
	};
}
