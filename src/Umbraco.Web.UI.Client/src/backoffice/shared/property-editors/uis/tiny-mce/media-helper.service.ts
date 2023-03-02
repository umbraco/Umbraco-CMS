// TODO => very much temporary

export class UmbMediaHelper {
    // Transplanted from mediahelper
	scaleToMaxSize(maxSize: number, width: number, height: number) {
		const retval = { width, height };

		const maxWidth = maxSize; // Max width for the image
		const maxHeight = maxSize; // Max height for the image
		let ratio = 0; // Used for aspect ratio

		// Check if the current width is larger than the max
		if (width > maxWidth) {
			ratio = maxWidth / width; // get ratio for scaling image

			retval.width = maxWidth;
			retval.height = height * ratio;

			height = height * ratio; // Reset height to match scaled image
			width = width * ratio; // Reset width to match scaled image
		}

		// Check if current height is larger than max
		if (height > maxHeight) {
			ratio = maxHeight / height; // get ratio for scaling image

			retval.height = maxHeight;
			retval.width = width * ratio;
			width = width * ratio; // Reset width to match scaled image
		}

		return retval;
	}

	async getProcessedImageUrl(imagePath: string, options: any) {
		if (!options) {
			return imagePath;
		}

		const result = await fetch('/umbraco/management/api/v1/images/GetProcessedImageUrl');

		return result;
	}
}
