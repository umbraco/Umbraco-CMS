import type { UmbScriptTreeItemModel } from '../tree/index.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import {
	type UmbTreePickerModalValue,
	type UmbTreePickerModalData,
	UMB_TREE_PICKER_MODAL_ALIAS,
} from '@umbraco-cms/backoffice/tree';

export type UmbScriptPickerModalData = UmbTreePickerModalData<UmbScriptTreeItemModel>;
export type UmbScriptPickerModalValue = UmbTreePickerModalValue;

export const UMB_SCRIPT_PICKER_MODAL = new UmbModalToken<UmbScriptPickerModalData, UmbScriptPickerModalValue>(
	UMB_TREE_PICKER_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
		data: {
			treeAlias: 'Umb.Tree.Script',
			hideTreeRoot: true,
		},
	},
);
