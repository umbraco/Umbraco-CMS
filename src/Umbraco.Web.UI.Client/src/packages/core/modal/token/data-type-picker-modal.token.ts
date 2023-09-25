import { FolderTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbModalToken, UmbTreePickerModalData, UmbPickerModalValue } from '@umbraco-cms/backoffice/modal';

export type UmbDataTypePickerModalData = UmbTreePickerModalData<FolderTreeItemResponseModel>;
export type UmbDataTypePickerModalValue = UmbPickerModalValue;

export const UMB_DATA_TYPE_PICKER_MODAL = new UmbModalToken<UmbDataTypePickerModalData, UmbDataTypePickerModalValue>(
	'Umb.Modal.TreePicker',
	{
		type: 'sidebar',
		size: 'small',
	},
	{
		treeAlias: 'Umb.Tree.DataTypes',
	},
);
