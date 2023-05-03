import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDataTypePickerModalData {
	selection?: Array<string | null>;
	multiple?: boolean;
}

export interface UmbDataTypePickerModalResult {
	selection: Array<string | null>;
}

export const UMB_DATA_TYPE_PICKER_MODAL = new UmbModalToken<UmbDataTypePickerModalData, UmbDataTypePickerModalResult>(
	'Umb.Modal.DataTypePicker',
	{
		type: 'sidebar',
		size: 'small',
	}
);
