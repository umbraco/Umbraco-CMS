import { UMB_TREE_PICKER_MODAL_ALIAS } from './constants.js';
import type { UmbPickerModalData, UmbPickerModalValue, UmbWorkspaceModalData } from '@umbraco-cms/backoffice/modal';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbTreePickerModalCreateActionData extends UmbWorkspaceModalData {}
export interface UmbTreePickerModalData<TreeItemType = any> extends UmbPickerModalData<TreeItemType> {
	treeAlias?: string;
	// Consider if it makes sense to move this into the UmbPickerModalData interface, but for now this is a TreePicker feature. [NL]
	createAction?: UmbTreePickerModalCreateActionData;
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
