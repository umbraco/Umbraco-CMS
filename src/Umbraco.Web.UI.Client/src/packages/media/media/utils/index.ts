// TODO => this is NOT a full reimplementation of the existing media helper service, currently
// contains only functions referenced by the TinyMCE editor
// TODO: This should not be done in this way, we need to split this into separate defined helper methods. This is also very specific to TinyMCE, so should be named that way. [NL]

import { getProcessedImageUrl } from '@umbraco-cms/backoffice/utils';
import type { Editor } from '@umbraco-cms/backoffice/external/tinymce';

/**
 * Sizes an image in the editor
 * @param editor
 * @param imageDomElement
 * @param imgUrl
 */
export async function sizeImageInEditor(editor: Editor, imageDomElement: HTMLElement, imgUrl?: string) {
	const size = editor.dom.getSize(imageDomElement);
	const maxImageSize = editor.options.get('maxImageSize');

	if (maxImageSize && maxImageSize > 0) {
		const newSize = scaleToMaxSize(maxImageSize, size.w, size.h);

		editor.dom.setAttribs(imageDomElement, { width: Math.round(newSize.width), height: Math.round(newSize.height) });

		// Images inserted via Media Picker will have a URL we can use for ImageResizer QueryStrings
		// Images pasted/dragged in are not persisted to media until saved & thus will need to be added
		if (imgUrl) {
			const resizedImgUrl = await getProcessedImageUrl(imgUrl, {
				width: newSize.width,
				height: newSize.height,
			});

			editor.dom.setAttrib(imageDomElement, 'data-mce-src', resizedImgUrl);
		}

		editor.execCommand('mceAutoResize', false);
	}
}

/**
 * Scales an image to the max size
 * @param maxSize
 * @param width
 * @param height
 */
export function scaleToMaxSize(maxSize: number, width: number, height: number) {
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
 * Uploads blob images to the server
 * @param editor
 * @param newContent
 */
export async function uploadBlobImages(editor: Editor, newContent?: string) {
	const content = newContent ?? editor.getContent();

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
			const tmpLocation = sessionStorage.getItem(`tinymce__${imgSrc}`);

			// Select the img & add new attr which we can search for
			// When its being persisted in RTE property editor
			// To create a media item & delete this tmp one etc
			editor.dom.setAttrib(img, 'data-tmpimg', tmpLocation);

			// Resize the image to the max size configured
			// NOTE: no imagesrc passed into func as the src is blob://...
			// We will append ImageResizing Querystrings on perist to DB with node save
			sizeImageInEditor(editor, img);
		});

		// Get all img where src starts with blob: AND does NOT have a data=tmpimg attribute
		// This is most likely seen as a duplicate image that has already been uploaded
		// editor.uploadImages() does not give us any indiciation that the image been uploaded already
		const blobImageWithNoTmpImgAttribute = editor.dom.select('img[src^="blob:"]:not([data-tmpimg])');

		//For each of these selected items
		blobImageWithNoTmpImgAttribute.forEach((imageElement) => {
			const blobSrcUri = editor.dom.getAttrib(imageElement, 'src');

			// Find the same image uploaded (Should be in SessionStorage)
			// May already exist in the editor as duplicate image
			// OR added to the RTE, deleted & re-added again
			// So lets fetch the tempurl out of sessionStorage for that blob URI item
			const tmpLocation = sessionStorage.getItem(`tinymce__${blobSrcUri}`);
			if (tmpLocation) {
				sizeImageInEditor(editor, imageElement);
				editor.dom.setAttrib(imageElement, 'data-tmpimg', tmpLocation);
			}
		});
	}
}
