import { FolderTreeItemResponseModel } from 'src/libs/backend-api';
import { UmbModalToken, UmbTreePickerModalData, UmbPickerModalResult } from 'src/libs/modal';

export type UmbDataTypePickerModalData = UmbTreePickerModalData<FolderTreeItemResponseModel>;
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
