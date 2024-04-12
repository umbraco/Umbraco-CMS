import { UMB_TREE_PICKER_MODAL_ALIAS } from './constants.js';
import type { UmbPickerModalData, UmbPickerModalValue } from '@umbraco-cms/backoffice/modal';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbTreePickerModalData<TreeItemType = any> extends UmbPickerModalData<TreeItemType> {
	treeAlias?: string;
}

export interface UmbTreePickerModalValue extends UmbPickerModalValue {}

export const UMB_TREE_PICKER_MODAL = new UmbModalToken<UmbTreePickerModalData, UmbTreePickerModalValue>(
	UMB_TREE_PICKER_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
