import type { UmbImageCropperPropertyEditorValue } from '../../../components';

export const UMB_IMAGE_CROPPER_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.ImageCropper' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_IMAGE_CROPPER_PROPERTY_EDITOR_VALUE_TYPE]: UmbImageCropperPropertyEditorValue | undefined;
	}
}
