import { FolderTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbModalToken, UmbPickerModalData, UmbPickerModalResult } from '@umbraco-cms/backoffice/modal';

export type UmbDataTypePickerModalData = UmbPickerModalData<FolderTreeItemResponseModel>;
export type UmbDataTypePickerModalResult = UmbPickerModalResult;

export const UMB_DATA_TYPE_PICKER_MODAL = new UmbModalToken<UmbDataTypePickerModalData, UmbDataTypePickerModalResult>(
	'Umb.Modal.DataTypePicker',
	{
		type: 'sidebar',
		size: 'small',
	}
);
