/**
 * Get the dimensions of an image from a URL.
 * @param {string} url The URL of the image. It can be a local file (blob url) or a remote file.
 * @returns {Promise<{width: number, height: number}>} The width and height of the image as downloaded from the URL.
 */
export function imageSize(url: string): Promise<{ width: number; height: number }> {
	const img = new Image();

	const promise = new Promise<{ width: number; height: number }>((resolve, reject) => {
		img.onload = () => {
			// Natural size is the actual image size regardless of rendering.
			// The 'normal' `width`/`height` are for the **rendered** size.
			const width = img.naturalWidth;
			const height = img.naturalHeight;

			// Resolve promise with the width and height
			resolve({ width, height });
		};

		// Reject promise on error
		img.onerror = reject;
	});

	// Setting the source makes it start downloading and eventually call `onload`
	img.src = url;

	return promise;
}
