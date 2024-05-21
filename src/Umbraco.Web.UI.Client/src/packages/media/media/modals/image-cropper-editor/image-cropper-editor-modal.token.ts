import type { UmbImageCropperPropertyEditorValue } from '../../components/index.js';
import type { UmbCropModel } from '../../property-editors/index.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbImageCropperEditorModalData {
	unique: string;
	focalPointEnabled: boolean;
	cropOptions: Array<UmbCropModel>;
}

export interface UmbImageCropperEditorModalValue extends UmbImageCropperPropertyEditorValue {}

export const UMB_IMAGE_CROPPER_EDITOR_MODAL = new UmbModalToken<
	UmbImageCropperEditorModalData,
	UmbImageCropperEditorModalValue
>('Umb.Modal.ImageCropperEditor', {
	modal: {
		type: 'sidebar',
		size: 'large',
	},
});
