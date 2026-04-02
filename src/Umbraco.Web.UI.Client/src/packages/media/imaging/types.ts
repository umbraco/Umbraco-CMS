import { ImageCropModeModel as UmbImagingCropMode } from '@umbraco-cms/backoffice/external/backend-api';

export { UmbImagingCropMode };

export interface UmbImagingResizeModel {
	height?: number;
	width?: number;
	mode?: UmbImagingCropMode;
	format?: string;
}

/**
 * @deprecated use `UmbImagingResizeModel` instead
 */
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbImagingModel extends UmbImagingResizeModel {}
