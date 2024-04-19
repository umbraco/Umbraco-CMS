import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbMediaPickerModalData {
	selection?: string[];
	multiple?: boolean;
}

export type UmbMediaPickerModalValue = {
	selection: string[];
};

export const UMB_MEDIA_PICKER_MODAL = new UmbModalToken<UmbMediaPickerModalData, UmbMediaPickerModalValue>(
	'Umb.Modal.MediaPicker',
	{
		modal: {
			type: 'sidebar',
			size: 'medium',
		},
	},
);
