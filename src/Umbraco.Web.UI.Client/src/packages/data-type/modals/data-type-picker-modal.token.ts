import {
	type UmbTreePickerModalValue,
	type UmbTreePickerModalData,
	type UmbUniqueTreeItemModel,
	UMB_TREE_PICKER_MODAL_ALIAS,
} from '@umbraco-cms/backoffice/tree';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbDataTypePickerModalData = UmbTreePickerModalData<UmbUniqueTreeItemModel>;
export type UmbDataTypePickerModalValue = UmbTreePickerModalValue;

export const UMB_DATA_TYPE_PICKER_MODAL = new UmbModalToken<UmbDataTypePickerModalData, UmbDataTypePickerModalValue>(
	UMB_TREE_PICKER_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
		data: {
			treeAlias: 'Umb.Tree.DataType',
		},
	},
);
