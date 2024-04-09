import { UmbModalToken } from './modal-token.js';
import type { UmbPickerModalValue, UmbTreePickerModalData } from '@umbraco-cms/backoffice/modal';
import type { UmbMediaTreeItemModel } from '@umbraco-cms/backoffice/media';

export type UmbMediaTreePickerModalData = UmbTreePickerModalData<UmbMediaTreeItemModel>;
export type UmbMediaTreePickerModalValue = UmbPickerModalValue;

export const UMB_MEDIA_TREE_PICKER_MODAL = new UmbModalToken<UmbMediaTreePickerModalData, UmbMediaTreePickerModalValue>(
	'Umb.Modal.TreePicker',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
		data: {
			treeAlias: 'Umb.Tree.Media',
		},
	},
);
