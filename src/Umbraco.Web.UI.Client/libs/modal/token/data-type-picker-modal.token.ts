import { UmbModalToken, UmbPickerModalResult, UmbTreePickerModalData } from '@umbraco-cms/backoffice/modal';

export type UmbDataTypePickerModalData = UmbTreePickerModalData<any>;
export type UmbDataTypePickerModalResult = UmbPickerModalResult;

export const UMB_DATA_TYPE_PICKER_MODAL = new UmbModalToken<UmbDataTypePickerModalData, UmbDataTypePickerModalResult>(
	'Umb.Modal.TreePicker',
	{
		type: 'sidebar',
		size: 'small',
	},
	{
		treeAlias: 'Umb.Tree.DataTypes',
	}
);
