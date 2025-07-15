import type { UmbAllowedMediaTypeModel } from '@umbraco-cms/backoffice/media-type';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDropzoneMediaTypePickerModalData {
	options: Array<UmbAllowedMediaTypeModel>;
	files?: Array<File>;
}

export type UmbDropzoneMediaTypePickerModalValue = {
	mediaTypeUnique: string | undefined;
};

export const UMB_DROPZONE_MEDIA_TYPE_PICKER_MODAL = new UmbModalToken<
	UmbDropzoneMediaTypePickerModalData,
	UmbDropzoneMediaTypePickerModalValue
>('Umb.Modal.Dropzone.MediaTypePicker', {
	modal: {
		type: 'dialog',
	},
});
