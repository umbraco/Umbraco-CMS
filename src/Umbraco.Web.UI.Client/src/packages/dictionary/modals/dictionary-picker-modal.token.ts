import type { UmbUniqueTreeItemModel } from '../../core/tree/types.js';
import { UmbModalToken } from '../../core/modal/token/modal-token.js';
import {
	type UmbTreePickerModalValue,
	type UmbTreePickerModalData,
	UMB_TREE_PICKER_MODAL_ALIAS,
} from '@umbraco-cms/backoffice/tree';

export type UmbDictionaryPickerModalData = UmbTreePickerModalData<UmbUniqueTreeItemModel>;
export type UmbDictionaryPickerModalValue = UmbTreePickerModalValue;

export const UMB_DICTIONARY_PICKER_MODAL = new UmbModalToken<
	UmbDictionaryPickerModalData,
	UmbDictionaryPickerModalValue
>(UMB_TREE_PICKER_MODAL_ALIAS, {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
	data: {
		hideTreeRoot: true,
		treeAlias: 'Umb.Tree.Dictionary',
	},
});
