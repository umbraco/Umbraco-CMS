// TODO => this is NOT a full reimplementation of the existing media helper service, currently
// contains only functions referenced by the TinyMCE editor
// TODO: This should not be done in this way, we need to split this into seperate defined helper methods. This is also very specific to TinyMCE, so should be named that way.

import { Editor, EditorEvent } from 'tinymce';

export class UmbMediaHelper {
	/**
	 *
	 * @param editor
	 * @param imageDomElement
	 * @param imgUrl
	 */
	async sizeImageInEditor(editor: Editor, imageDomElement: HTMLElement, imgUrl?: string) {
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

			editor.execCommand('mceAutoResize', false);
		}
	}

	/**
	 *
	 * @param maxSize
	 * @param width
	 * @param height
	 * @returns
	 */
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

	/**
	 *
	 * @param imagePath
	 * @param options
	 * @returns
	 */
	async getProcessedImageUrl(imagePath: string, options: any) {
		if (!options) {
			return imagePath;
		}

		// TODO => use backend cli when available
		const result = await fetch('/umbraco/management/api/v1/images/GetProcessedImageUrl');
		const url = (await result.json()) as string;

		return url;
	}

	/**
	 *
	 * @param editor
	 */
	async uploadBlobImages(editor: Editor) {
		const content = editor.getContent();

		// Upload BLOB images (dragged/pasted ones)
		// find src attribute where value starts with `blob:`
		// search is case-insensitive and allows single or double quotes
		if (content.search(/src=["']blob:.*?["']/gi) !== -1) {
			const data = await editor.uploadImages();
			// Once all images have been uploaded
			data.forEach((item) => {
				// Skip items that failed upload
				if (item.status === false) {
					return;
				}

				// Select img element
				const img = item.element;

				// Get img src
				const imgSrc = img.getAttribute('src');
				const tmpLocation = localStorage.get(`tinymce__${imgSrc}`);

				// Select the img & add new attr which we can search for
				// When its being persisted in RTE property editor
				// To create a media item & delete this tmp one etc
				editor.dom.setAttrib(img, 'data-tmpimg', tmpLocation);

				// Resize the image to the max size configured
				// NOTE: no imagesrc passed into func as the src is blob://...
				// We will append ImageResizing Querystrings on perist to DB with node save
				this.sizeImageInEditor(editor, img);
			});

			// Get all img where src starts with blob: AND does NOT have a data=tmpimg attribute
			// This is most likely seen as a duplicate image that has already been uploaded
			// editor.uploadImages() does not give us any indiciation that the image been uploaded already
			const blobImageWithNoTmpImgAttribute = editor.dom.select('img[src^="blob:"]:not([data-tmpimg])');

			//For each of these selected items
			blobImageWithNoTmpImgAttribute.forEach((imageElement) => {
				const blobSrcUri = editor.dom.getAttrib(imageElement, 'src');

				// Find the same image uploaded (Should be in LocalStorage)
				// May already exist in the editor as duplicate image
				// OR added to the RTE, deleted & re-added again
				// So lets fetch the tempurl out of localstorage for that blob URI item

				const tmpLocation = localStorage.get(`tinymce__${blobSrcUri}`);
				if (tmpLocation) {
					this.sizeImageInEditor(editor, imageElement);
					editor.dom.setAttrib(imageElement, 'data-tmpimg', tmpLocation);
				}
			});
		}

		if (window.Umbraco?.Sys.ServerVariables.umbracoSettings.sanitizeTinyMce) {
			/** prevent injecting arbitrary JavaScript execution in on-attributes. */
			const allNodes = Array.from(editor.dom.doc.getElementsByTagName('*'));
			allNodes.forEach((node) => {
				for (let i = 0; i < node.attributes.length; i++) {
					if (node.attributes[i].name.startsWith('on')) {
						node.removeAttribute(node.attributes[i].name);
					}
				}
			});
		}
	}

	/**
	 *
	 * @param e
	 * @returns
	 */
	async onResize(
		e: EditorEvent<{
			target: HTMLElement;
			width: number;
			height: number;
			origin: string;
		}>
	) {
		const srcAttr = e.target.getAttribute('src');

		if (!srcAttr) {
			return;
		}

		const path = srcAttr.split('?')[0];
		const resizedPath = await this.getProcessedImageUrl(path, {
			width: e.width,
			height: e.height,
			mode: 'max',
		});

		e.target.setAttribute('data-mce-src', resizedPath);
	}
}
