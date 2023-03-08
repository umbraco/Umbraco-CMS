import { UmbModalToken } from 'libs/modal';

export interface UmbMediaPickerModalData {
	multiple?: boolean;
	selection: Array<string>;
}

export const UMB_MEDIA_PICKER_MODAL_TOKEN = new UmbModalToken<UmbMediaPickerModalData>('Umb.Modal.MediaPicker', {
	type: 'sidebar',
	size: 'small',
});
