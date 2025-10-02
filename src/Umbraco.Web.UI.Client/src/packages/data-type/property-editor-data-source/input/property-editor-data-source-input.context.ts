import type { UmbPropertyEditorDataSourceItemModel } from '../item/types.js';
import { UMB_PROPERTY_EDITOR_DATA_SOURCE_ITEM_REPOSITORY_ALIAS } from '../item/constants.js';
import { UMB_PROPERTY_EDITOR_DATA_SOURCE_COLLECTION_MENU_ALIAS } from '../collection/constants.js';
import { UMB_PROPERTY_EDITOR_DATA_SOURCE_SEARCH_PROVIDER_ALIAS } from '../search/constants.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UMB_COLLECTION_ITEM_PICKER_MODAL_ALIAS,
	type UmbCollectionItemPickerModalData,
	type UmbCollectionItemPickerModalValue,
} from '@umbraco-cms/backoffice/collection';

const modalToken = new UmbModalToken<UmbCollectionItemPickerModalData, UmbCollectionItemPickerModalValue>(
	UMB_COLLECTION_ITEM_PICKER_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
		data: {
			collectionMenuAlias: UMB_PROPERTY_EDITOR_DATA_SOURCE_COLLECTION_MENU_ALIAS,
		},
	},
);

export class UmbPropertyEditorDataSourcePickerInputContext extends UmbPickerInputContext<
	UmbPropertyEditorDataSourceItemModel,
	UmbPropertyEditorDataSourceItemModel,
	UmbCollectionItemPickerModalData<UmbPropertyEditorDataSourceItemModel>,
	UmbCollectionItemPickerModalValue
> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_PROPERTY_EDITOR_DATA_SOURCE_ITEM_REPOSITORY_ALIAS, modalToken);
	}

	#dataSourceTypes: Array<string> = [];

	setDataSourceTypes(value: Array<string>) {
		this.#dataSourceTypes = value;
	}

	getDataSourceTypes(): Array<string> {
		return this.#dataSourceTypes;
	}

	override async openPicker(
		pickerData?: Partial<UmbCollectionItemPickerModalData<UmbPropertyEditorDataSourceItemModel>>,
	) {
		const combinedPickerData = {
			...pickerData,
		};

		// set default search data
		if (!pickerData?.search) {
			combinedPickerData.search = {
				providerAlias: UMB_PROPERTY_EDITOR_DATA_SOURCE_SEARCH_PROVIDER_ALIAS,
				...pickerData?.search,
			};
		}

		// pass allowedContentTypes to the search request args
		combinedPickerData.search!.queryParams = {
			dataSourceTypes: this.getDataSourceTypes(),
			...pickerData?.search?.queryParams,
		};

		await super.openPicker(combinedPickerData);
	}
}
