// TODO => very much temporary

export class UmbMediaHelper {
	async sizeImageInEditor(editor: any, imageDomElement: HTMLElement, imgUrl?: string) {
		const size = editor.dom.getSize(imageDomElement);
		const maxImageSize = editor.options.get('maxImageSize');

		if (maxImageSize && maxImageSize > 0) {
			const newSize = this.scaleToMaxSize(maxImageSize, size.w, size.h);

			editor.dom.setAttribs(imageDomElement, { width: Math.round(newSize.width), height: Math.round(newSize.height) });

			// Images inserted via Media Picker will have a URL we can use for ImageResizer QueryStrings
			// Images pasted/dragged in are not persisted to media until saved & thus will need to be added
			if (imgUrl) {
				const resizedImgUrl = await this.getProcessedImageUrl(imgUrl, {
					width: newSize.width,
					height: newSize.height,
				});

				editor.dom.setAttrib(imageDomElement, 'data-mce-src', resizedImgUrl);
			}

			editor.execCommand('mceAutoResize', false, null, null);
		}
	}

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
