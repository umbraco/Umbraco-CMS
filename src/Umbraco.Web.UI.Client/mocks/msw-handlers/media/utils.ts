import type { UmbMockMediaModel } from '../../data/mock-data-set.types.js';

/**
 * Extracts the file URL from a media item's `umbracoFile` property value.
 * Handles both ImageCropper (`{ src, focalPoint, crops }`) and UploadField (string path) formats.
 * @param {UmbMockMediaModel} item The media mock model to extract the file URL from.
 * @returns {string | null} The file URL string, or null if no file value is found.
 */
export function getMediaFileUrl(item: UmbMockMediaModel): string | null {
	const fileValue = item.values.find((v) => v.alias === 'umbracoFile');
	if (!fileValue?.value) return null;

	// ImageCropper stores { src, focalPoint, crops }, UploadField stores the path directly
	if (typeof fileValue.value === 'object' && 'src' in (fileValue.value as Record<string, unknown>)) {
		return (fileValue.value as { src: string }).src;
	}
	if (typeof fileValue.value === 'string') {
		return fileValue.value;
	}
	return null;
}
