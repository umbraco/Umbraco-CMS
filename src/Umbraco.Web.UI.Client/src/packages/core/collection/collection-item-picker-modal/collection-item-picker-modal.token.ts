import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import { UMB_COLLECTION_ITEM_PICKER_MODAL_ALIAS } from './constants.js';
import type { UmbCollectionItemPickerModalData, UmbCollectionItemPickerModalValue } from './types.js';

export const UMB_COLLECTION_ITEM_PICKER_MODAL = new UmbModalToken<
	UmbCollectionItemPickerModalData,
	UmbCollectionItemPickerModalValue
>(UMB_COLLECTION_ITEM_PICKER_MODAL_ALIAS, {
	modal: { type: 'sidebar', size: 'medium' },
});
