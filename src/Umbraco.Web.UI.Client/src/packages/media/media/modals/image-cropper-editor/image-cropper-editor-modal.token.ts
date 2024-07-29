import type { UmbImageCropperCrop } from '../../components/index.js';
import type { UmbCropModel } from '../../property-editors/types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbImageCropperEditorModalData<ItemType> {
	key: string;
	unique: string;
	hideFocalPoint: boolean;
	cropOptions: Array<UmbCropModel>;
	pickableFilter?: (item: ItemType) => boolean;
}

export interface UmbImageCropperEditorModalValue {
	key: string;
	unique: string;
	crops: Array<UmbImageCropperCrop>;
	focalPoint: { left: number; top: number };
}

export const UMB_IMAGE_CROPPER_EDITOR_MODAL = new UmbModalToken<
	UmbImageCropperEditorModalData<any>,
	UmbImageCropperEditorModalValue
>('Umb.Modal.ImageCropperEditor', {
	modal: {
		type: 'sidebar',
		size: 'full',
	},
});
