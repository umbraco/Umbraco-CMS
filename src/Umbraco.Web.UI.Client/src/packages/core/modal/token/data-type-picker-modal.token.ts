import type { UmbPickerModalValue, UmbTreePickerModalData } from '../types.js';
import { UmbModalToken } from './modal-token.js';
import type { UmbUniqueTreeItemModel } from '@umbraco-cms/backoffice/tree';

export type UmbDataTypePickerModalData = UmbTreePickerModalData<UmbUniqueTreeItemModel>;
export type UmbDataTypePickerModalValue = UmbPickerModalValue;

export const UMB_DATA_TYPE_PICKER_MODAL = new UmbModalToken<UmbDataTypePickerModalData, UmbDataTypePickerModalValue>(
	'Umb.Modal.TreePicker',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
		data: {
			treeAlias: 'Umb.Tree.DataTypes',
		},
	},
);
