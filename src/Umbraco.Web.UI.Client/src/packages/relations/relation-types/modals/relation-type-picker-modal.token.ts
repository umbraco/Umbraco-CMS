import type { UmbRelationTypeCollectionItemModel } from '../collection/types.js';
import { UMB_RELATION_TYPE_COLLECTION_MENU_ALIAS } from '../collection/constants.js';
import {
	UMB_COLLECTION_ITEM_PICKER_MODAL_ALIAS,
	type UmbCollectionItemPickerModalData,
	type UmbCollectionItemPickerModalValue,
} from '@umbraco-cms/backoffice/collection';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbRelationTypePickerModalData = UmbCollectionItemPickerModalData<UmbRelationTypeCollectionItemModel>;

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbRelationTypePickerModalValue extends UmbCollectionItemPickerModalValue {}

export const UMB_RELATION_TYPE_PICKER_MODAL = new UmbModalToken<
	UmbRelationTypePickerModalData,
	UmbRelationTypePickerModalValue
>(UMB_COLLECTION_ITEM_PICKER_MODAL_ALIAS, {
	modal: {
		type: 'sidebar',
		size: 'medium',
	},
	data: {
		collection: {
			menuAlias: UMB_RELATION_TYPE_COLLECTION_MENU_ALIAS,
		},
	},
});
