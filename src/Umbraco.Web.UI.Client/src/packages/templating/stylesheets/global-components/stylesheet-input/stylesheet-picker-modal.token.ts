import type { UmbStylesheetTreeItemModel } from '../../tree/types.js';
import {
	type UmbTreePickerModalValue,
	type UmbTreePickerModalData,
	UMB_TREE_PICKER_MODAL_ALIAS,
} from '@umbraco-cms/backoffice/tree';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbStylesheetPickerModalData = UmbTreePickerModalData<UmbStylesheetTreeItemModel>;
export type UmbStylesheetPickerModalValue = UmbTreePickerModalValue;

export const UMB_STYLESHEET_PICKER_MODAL = new UmbModalToken<
	UmbStylesheetPickerModalData,
	UmbStylesheetPickerModalValue
>(UMB_TREE_PICKER_MODAL_ALIAS, {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
	data: {
		treeAlias: 'Umb.Tree.Stylesheet',
		hideTreeRoot: true,
	},
});
