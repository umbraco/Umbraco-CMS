import { UMB_ELEMENT_TREE_ALIAS } from '../tree/constants.js';
import type { UmbElementTreeItemModel } from '../tree/types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import { UMB_TREE_PICKER_MODAL_ALIAS } from '@umbraco-cms/backoffice/tree';
import type { UmbTreePickerModalValue, UmbTreePickerModalData } from '@umbraco-cms/backoffice/tree';

export type UmbElementPickerModalData = UmbTreePickerModalData<UmbElementTreeItemModel>;
export type UmbElementPickerModalValue = UmbTreePickerModalValue;

export const UMB_ELEMENT_PICKER_MODAL = new UmbModalToken<UmbElementPickerModalData, UmbElementPickerModalValue>(
	UMB_TREE_PICKER_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
		data: {
			treeAlias: UMB_ELEMENT_TREE_ALIAS,
		},
	},
);
