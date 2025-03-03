/**
 * Get the dimensions of an image from a URL.
 * @param {string} url The URL of the image. It can be a local file (blob url) or a remote file.
 * @param {{maxWidth?: number}} opts Options for the image size.
 * @param {number} opts.maxWidth The maximum width of the image. If the image is wider than this, it will be scaled down to this width while keeping the aspect ratio.
 * @returns {Promise<{width: number, height: number, naturalWidth: number, naturalHeight: number}>} The width and height of the image as downloaded from the URL. The width and height can differ from the natural numbers if maxImageWidth is given.
 */
export function imageSize(
	url: string,
	opts?: { maxWidth?: number },
): Promise<{ width: number; height: number; naturalWidth: number; naturalHeight: number }> {
	const img = new Image();

	const promise = new Promise<{ width: number; height: number; naturalWidth: number; naturalHeight: number }>(
		(resolve, reject) => {
			img.onload = () => {
				// Natural size is the actual image size regardless of rendering.
				// The 'normal' `width`/`height` are for the **rendered** size.
				const naturalWidth = img.naturalWidth;
				const naturalHeight = img.naturalHeight;
				let width = naturalWidth;
				let height = naturalHeight;

				if (opts?.maxWidth && opts.maxWidth > 0 && width > opts?.maxWidth) {
					const ratio = opts.maxWidth / naturalWidth;
					width = opts.maxWidth;
					height = Math.round(naturalHeight * ratio);
				}

				// Resolve promise with the width and height
				resolve({ width, height, naturalWidth, naturalHeight });
			};

			// Reject promise on error
			img.onerror = reject;
		},
	);

	// Setting the source makes it start downloading and eventually call `onload`
	img.src = url;

	return promise;
}
