import type { UmbScriptTreeItemModel } from '../tree/index.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbPickerModalValue, UmbTreePickerModalData } from '@umbraco-cms/backoffice/modal';

export type UmbScriptPickerModalData = UmbTreePickerModalData<UmbScriptTreeItemModel>;
export type UmbScriptPickerModalValue = UmbPickerModalValue;

export const UMB_SCRIPT_PICKER_MODAL = new UmbModalToken<UmbScriptPickerModalData, UmbScriptPickerModalValue>(
	'Umb.Modal.TreePicker',
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
