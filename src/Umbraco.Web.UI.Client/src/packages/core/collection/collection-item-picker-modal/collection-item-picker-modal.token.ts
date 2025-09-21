import type { UmbCollectionItemPickerModalData, UmbCollectionItemPickerModalValue } from './types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export const UMB_COLLECTION_ITEM_PICKER_MODAL = new UmbModalToken<
	UmbCollectionItemPickerModalData,
	UmbCollectionItemPickerModalValue
>('Umb.Modal.CollectionItemPicker', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
