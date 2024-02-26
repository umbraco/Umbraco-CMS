import { UMB_STATIC_FILE_TREE_ALIAS } from '../tree/manifests.js';
import type { StaticFileItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbPickerModalValue, UmbTreePickerModalData } from '@umbraco-cms/backoffice/modal';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbStaticFilePickerModalData = UmbTreePickerModalData<StaticFileItemResponseModel>;
export type UmbStaticFilePickerModalValue = UmbPickerModalValue;

export const UMB_STATIC_FILE_PICKER_MODAL = new UmbModalToken<
	UmbStaticFilePickerModalData,
	UmbStaticFilePickerModalValue
>('Umb.Modal.TreePicker', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
	data: {
		treeAlias: UMB_STATIC_FILE_TREE_ALIAS,
	},
});
