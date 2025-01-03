import { UMB_STATIC_FILE_TREE_ALIAS } from '../tree/constants.js';
import type { UmbStaticFileItemModel } from '../repository/item/types.js';
import {
	type UmbTreePickerModalValue,
	type UmbTreePickerModalData,
	UMB_TREE_PICKER_MODAL_ALIAS,
} from '@umbraco-cms/backoffice/tree';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbStaticFilePickerModalData = UmbTreePickerModalData<UmbStaticFileItemModel>;
export type UmbStaticFilePickerModalValue = UmbTreePickerModalValue;

export const UMB_STATIC_FILE_PICKER_MODAL = new UmbModalToken<
	UmbStaticFilePickerModalData,
	UmbStaticFilePickerModalValue
>(UMB_TREE_PICKER_MODAL_ALIAS, {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
	data: {
		treeAlias: UMB_STATIC_FILE_TREE_ALIAS,
	},
});
