import type { UmbUniqueTreeItemModel } from '../../core/tree/types.js';
import { UmbModalToken } from '../../core/modal/token/modal-token.js';
import type { UmbPickerModalValue, UmbTreePickerModalData } from '@umbraco-cms/backoffice/modal';

export type UmbDictionaryPickerModalData = UmbTreePickerModalData<UmbUniqueTreeItemModel>;
export type UmbDictionaryPickerModalValue = UmbPickerModalValue;

export const UMB_DICTIONARY_PICKER_MODAL = new UmbModalToken<
	UmbDictionaryPickerModalData,
	UmbDictionaryPickerModalValue
>('Umb.Modal.TreePicker', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
	data: {
		hideTreeRoot: true,
		treeAlias: 'Umb.Tree.Dictionary',
	},
});
