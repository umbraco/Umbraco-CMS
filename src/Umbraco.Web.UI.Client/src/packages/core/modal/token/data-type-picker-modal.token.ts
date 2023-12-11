import { UmbModalToken, UmbTreePickerModalData, UmbPickerModalValue } from '@umbraco-cms/backoffice/modal';
import { UmbEntityTreeItemModel } from '@umbraco-cms/backoffice/tree';

export type UmbDataTypePickerModalData = UmbTreePickerModalData<UmbEntityTreeItemModel>;
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
