import type { UmbEntityTreeItemModel } from '../../tree/types.js';
import { UmbModalToken } from './modal-token.js';
import type { UmbPickerModalValue, UmbTreePickerModalData } from '@umbraco-cms/backoffice/modal';

export type UmbDictionaryItemPickerModalData = UmbTreePickerModalData<UmbEntityTreeItemModel>;
export type UmbDictionaryItemPickerModalValue = UmbPickerModalValue;

export const UMB_DICTIONARY_ITEM_PICKER_MODAL = new UmbModalToken<
	UmbDictionaryItemPickerModalData,
	UmbDictionaryItemPickerModalValue
>('Umb.Modal.TreePicker', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
	data: {
		treeAlias: 'Umb.Tree.Dictionary',
	},
});
