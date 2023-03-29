import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbMediaPickerModalData {
	multiple?: boolean;
	selection: Array<string>;
}

export interface UmbMediaPickerModalResult {
	selection: Array<string>;
}

export const UMB_MEDIA_PICKER_MODAL = new UmbModalToken<UmbMediaPickerModalData, UmbMediaPickerModalResult>(
	'Umb.Modal.MediaPicker',
	{
		type: 'sidebar',
		size: 'small',
	}
);
