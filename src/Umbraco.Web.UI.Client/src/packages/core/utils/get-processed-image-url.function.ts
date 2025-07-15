import type { GetImagingResizeUrlsData } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * Returns the URL of the processed image.
 * @param {string} imagePath The path to the image.
 * @param {GetImagingResizeUrlsData} options The options for resizing the image.
 * @returns {Promise<string>} The URL of the processed image.
 */
export async function getProcessedImageUrl(
	imagePath: string,
	options: GetImagingResizeUrlsData['query'],
): Promise<string> {
	if (!options) {
		return imagePath;
	}

	const searchParams = new URLSearchParams({
		width: options.width?.toString() ?? '',
		height: options.height?.toString() ?? '',
		mode: options.mode ?? '',
	});

	// This should ideally use the ImagingService.getImagingResizeUrls method, but
	// that would require the GUID of the media item, which is not available here.
	const url = `${imagePath}?${searchParams.toString()}`;

	return url;
}
