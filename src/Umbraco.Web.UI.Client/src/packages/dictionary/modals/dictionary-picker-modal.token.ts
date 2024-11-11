import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import {
	type UmbTreePickerModalValue,
	type UmbTreePickerModalData,
	type UmbTreeItemModel,
	UMB_TREE_PICKER_MODAL_ALIAS,
} from '@umbraco-cms/backoffice/tree';

export type UmbDictionaryPickerModalData = UmbTreePickerModalData<UmbTreeItemModel>;
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
