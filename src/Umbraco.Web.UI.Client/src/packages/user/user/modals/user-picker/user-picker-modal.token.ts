import type { UmbUserDetailModel } from '../../types.js';
import { UMB_USER_COLLECTION_MENU_ALIAS } from '../../collection/constants.js';
import {
	UMB_COLLECTION_ITEM_PICKER_MODAL_ALIAS,
	type UmbCollectionItemPickerModalData,
	type UmbCollectionItemPickerModalValue,
} from '@umbraco-cms/backoffice/collection';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbUserPickerModalData = UmbCollectionItemPickerModalData<UmbUserDetailModel>;

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbUserPickerModalValue extends UmbCollectionItemPickerModalValue {}

export const UMB_USER_PICKER_MODAL = new UmbModalToken<UmbUserPickerModalData, UmbUserPickerModalValue>(
	UMB_COLLECTION_ITEM_PICKER_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
		data: {
			collection: {
				menuAlias: UMB_USER_COLLECTION_MENU_ALIAS,
			},
		},
	},
);
