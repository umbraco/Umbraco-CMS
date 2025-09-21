import { UMB_COLLECTION_ITEM_PICKER_MODAL_ALIAS } from '@umbraco-cms/backoffice/collection';
import type { UmbPropertyEditorDataSourceItemModel } from '../item/types.js';
import { UmbModalToken, type UmbPickerModalData, type UmbPickerModalValue } from '@umbraco-cms/backoffice/modal';
import { UMB_PROPERTY_EDITOR_DATA_SOURCE_COLLECTION_REPOSITORY_ALIAS } from '../constants.js';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbPropertyEditorDataSourcePickerModalData
	extends UmbPickerModalData<UmbPropertyEditorDataSourceItemModel> {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbPropertyEditorDataSourcePickerModalValue extends UmbPickerModalValue {}

export const UMB_PROPERTY_EDITOR_DATA_SOURCE_PICKER_MODAL = new UmbModalToken<
	UmbPropertyEditorDataSourcePickerModalData,
	UmbPropertyEditorDataSourcePickerModalValue
>(UMB_COLLECTION_ITEM_PICKER_MODAL_ALIAS, {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
	data: {
		collectionRepositoryAlias: UMB_PROPERTY_EDITOR_DATA_SOURCE_COLLECTION_REPOSITORY_ALIAS,
	},
});
