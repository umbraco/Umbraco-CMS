import type { UmbImageCropperPropertyEditorValue } from '../../../components/index.js';

export const UMB_IMAGE_CROPPER_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.ImageCropper' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_IMAGE_CROPPER_PROPERTY_EDITOR_VALUE_TYPE]: UmbImageCropperPropertyEditorValue | undefined;
	}
}
