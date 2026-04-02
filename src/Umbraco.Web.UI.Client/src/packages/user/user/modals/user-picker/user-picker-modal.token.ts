import type { UmbUserDetailModel } from '../../types.js';
import { UMB_USER_COLLECTION_ALIAS } from '../../collection/constants.js';
import type {
	UmbCollectionItemPickerModalData,
	UmbCollectionItemPickerModalValue,
} from '@umbraco-cms/backoffice/collection';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbUserPickerModalData = UmbCollectionItemPickerModalData<UmbUserDetailModel>;

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbUserPickerModalValue extends UmbCollectionItemPickerModalValue {}

export const UMB_USER_PICKER_MODAL = new UmbModalToken<UmbUserPickerModalData, UmbUserPickerModalValue>(
	/* TODO: use constant. We had to use the string directly here to avoid a circular dependency.
	 When we have removed the dataType (dependency on content) from the picker context we update this */
	'Umb.Modal.CollectionItemPicker',
	{
		modal: {
			type: 'sidebar',
			size: 'medium',
		},
		data: {
			collection: {
				alias: UMB_USER_COLLECTION_ALIAS,
			},
		},
	},
);
