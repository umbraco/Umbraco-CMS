import type { UmbPropertyEditorDataSourceItemModel } from '../item/types.js';
import { UMB_PROPERTY_EDITOR_DATA_SOURCE_COLLECTION_MENU_ALIAS } from '../constants.js';
import { UMB_PROPERTY_EDITOR_DATA_SOURCE_SEARCH_PROVIDER_ALIAS } from '../search/constants.js';
import {
	UMB_COLLECTION_ITEM_PICKER_MODAL_ALIAS,
	type UmbCollectionItemPickerModalData,
	type UmbCollectionItemPickerModalValue,
} from '@umbraco-cms/backoffice/collection';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbPropertyEditorDataSourcePickerModalData
	extends UmbCollectionItemPickerModalData<UmbPropertyEditorDataSourceItemModel> {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbPropertyEditorDataSourcePickerModalValue extends UmbCollectionItemPickerModalValue {}

export const UMB_PROPERTY_EDITOR_DATA_SOURCE_PICKER_MODAL = new UmbModalToken<
	UmbPropertyEditorDataSourcePickerModalData,
	UmbPropertyEditorDataSourcePickerModalValue
>(UMB_COLLECTION_ITEM_PICKER_MODAL_ALIAS, {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
	data: {
		collectionMenuAlias: UMB_PROPERTY_EDITOR_DATA_SOURCE_COLLECTION_MENU_ALIAS,
		search: {
			providerAlias: UMB_PROPERTY_EDITOR_DATA_SOURCE_SEARCH_PROVIDER_ALIAS,
		},
	},
});
