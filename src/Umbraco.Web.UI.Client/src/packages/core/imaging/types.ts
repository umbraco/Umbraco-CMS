import type { ImageCropModeModel } from '@umbraco-cms/backoffice/external/backend-api';

export interface UmbImagingModel {
	height?: number;
	width?: number;
	mode?: ImageCropModeModel;
}
