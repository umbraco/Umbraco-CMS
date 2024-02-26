import { UmbModalToken } from './modal-token.js';
import type { UmbPickerModalValue, UmbTreePickerModalData } from '@umbraco-cms/backoffice/modal';
import type { UmbUniqueTreeItemModel } from '@umbraco-cms/backoffice/tree';

export type UmbMediaTypePickerModalData = UmbTreePickerModalData<UmbUniqueTreeItemModel>;
export type UmbMediaTypePickerModalValue = UmbPickerModalValue;

export const UMB_MEDIA_TYPE_PICKER_MODAL = new UmbModalToken<UmbMediaTypePickerModalData, UmbMediaTypePickerModalValue>(
	'Umb.Modal.TreePicker',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
		data: {
			treeAlias: 'Umb.Tree.MediaType',
		},
	},
);
