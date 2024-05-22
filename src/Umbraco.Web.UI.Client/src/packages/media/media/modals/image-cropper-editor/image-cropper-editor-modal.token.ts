import type { UmbImageCropperCrop } from '../../components/index.js';
import type { UmbCropModel } from '../../property-editors/index.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbImageCropperEditorModalData {
	unique: string;
	focalPointEnabled: boolean;
	cropOptions: Array<UmbCropModel>;
}

export interface UmbImageCropperEditorModalValue {
	unique: string;
	crops: Array<UmbImageCropperCrop>;
	focalPoint: { left: number; top: number };
}

export const UMB_IMAGE_CROPPER_EDITOR_MODAL = new UmbModalToken<
	UmbImageCropperEditorModalData,
	UmbImageCropperEditorModalValue
>('Umb.Modal.ImageCropperEditor', {
	modal: {
		type: 'sidebar',
		size: 'large',
	},
});
