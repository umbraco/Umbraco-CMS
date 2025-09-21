import { UMB_COLLECTION_ITEM_PICKER_MODAL_ALIAS } from './constants.js';
import type { UmbPickerModalData, UmbPickerModalValue } from '@umbraco-cms/backoffice/modal';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbCollectionItemPickerModalData<CollectionItemType = UmbEntityModel>
	extends UmbPickerModalData<CollectionItemType> {
	collectionMenuAlias?: string;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbCollectionItemPickerModalValue extends UmbPickerModalValue {}

export const UMB_COLLECTION_ITEM_PICKER_MODAL = new UmbModalToken<
	UmbCollectionItemPickerModalData,
	UmbCollectionItemPickerModalValue
>(UMB_COLLECTION_ITEM_PICKER_MODAL_ALIAS, {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
