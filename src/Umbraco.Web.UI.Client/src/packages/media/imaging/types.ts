import { ImageCropModeModel as UmbImagingCropMode } from '@umbraco-cms/backoffice/external/backend-api';

export { UmbImagingCropMode };

export interface UmbImagingModel {
	height?: number;
	width?: number;
	mode?: UmbImagingCropMode;
}
