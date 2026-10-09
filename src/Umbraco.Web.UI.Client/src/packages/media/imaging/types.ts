import { ImageCropModeModel as UmbImagingCropMode } from '@umbraco-cms/backoffice/external/backend-api';

export { UmbImagingCropMode };

export interface UmbImagingResizeModel {
	height?: number;
	width?: number;
	mode?: UmbImagingCropMode;
	format?: string;
}

/**
 * Generates a cache key from an imaging resize configuration.
 * Used by both the imaging store and the request batcher to ensure consistent keying.
 * @param {UmbImagingResizeModel} model - The resize configuration
 * @returns {string} The cache key
 */
export function generateImagingCacheKey(model?: UmbImagingResizeModel): string {
	return model ? `${model.width}x${model.height};${model.mode};${model.format}` : 'generic';
}

/**
 * @deprecated use `UmbImagingResizeModel` instead
 */
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbImagingModel extends UmbImagingResizeModel {}
