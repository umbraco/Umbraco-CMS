/**
 * Get the dimensions of an image from a URL.
 * @param {string} url The URL of the image. It can be a local file (blob url) or a remote file.
 * @param {{maxWidth?: number, maxHeight?: number}} opts Options for the image size.
 * @param {number} opts.maxWidth The maximum width of the image.
 * @param {number} opts.maxHeight The maximum height of the image.
 * @returns {Promise<{width: number, height: number, naturalWidth: number, naturalHeight: number}>} The dimensions of the image.
 */
export function imageSize(
	url: string,
	opts?: { maxWidth?: number; maxHeight?: number },
): Promise<{ width: number; height: number; naturalWidth: number; naturalHeight: number }> {
	const img = new Image();

	const promise = new Promise<{ width: number; height: number; naturalWidth: number; naturalHeight: number }>(
		(resolve, reject) => {
			img.onload = () => {
				const naturalWidth = img.naturalWidth;
				const naturalHeight = img.naturalHeight;
				let width = naturalWidth;
				let height = naturalHeight;

				if ((opts?.maxWidth && opts.maxWidth > 0) || (opts?.maxHeight && opts.maxHeight > 0)) {
					const widthRatio = opts?.maxWidth ? opts.maxWidth / naturalWidth : 1;
					const heightRatio = opts?.maxHeight ? opts.maxHeight / naturalHeight : 1;
					const ratio = Math.min(widthRatio, heightRatio, 1); // Never upscale
					width = Math.round(naturalWidth * ratio);
					height = Math.round(naturalHeight * ratio);
				}

				resolve({ width, height, naturalWidth, naturalHeight });
			};
			img.onerror = reject;
		},
	);

	img.src = url;
	return promise;
}
