import type { ImageCropModeModel } from '@umbraco-cms/backoffice/external/backend-api';

export interface UmbImagingModel {
	uniques: Array<string>;
	height?: number;
	width?: number;
	mode?: ImageCropModeModel;
}
