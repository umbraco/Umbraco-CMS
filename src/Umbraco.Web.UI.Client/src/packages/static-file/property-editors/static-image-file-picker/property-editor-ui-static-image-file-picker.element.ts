import { UmbPropertyEditorUIStaticFilePickerElement } from '../static-file-picker/property-editor-ui-static-file-picker.element.js';
import { customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTemporaryFileConfigRepository } from '@umbraco-cms/backoffice/temporary-file';
import { UmbServerFilePathUniqueSerializer } from '@umbraco-cms/backoffice/server-file-system';

// SVG is rendered natively by browsers and was an allowed thumbnail format in v13, but it can
// never appear in the server's imageFileTypes because the imaging pipeline cannot process it (#20574).
const ADDITIONAL_IMAGE_FILE_TYPES = ['svg'];

/**
 * Decides whether a static file can be picked as an image.
 * @param {string} path The server file path of the item.
 * @param {boolean} isFolder Whether the item is a folder.
 * @param {Array<string> | undefined} imageFileTypes The allowed image file extensions (without leading dots). When undefined or empty, any file is allowed.
 * @returns {boolean} True if the item can be picked.
 */
export function isPickableImageFile(
	path: string,
	isFolder: boolean,
	imageFileTypes: Array<string> | undefined,
): boolean {
	if (isFolder) return false;
	if (!imageFileTypes || imageFileTypes.length === 0) return true;
	const lastDotIndex = path.lastIndexOf('.');
	if (lastDotIndex === -1) return false;
	const extension = path.substring(lastDotIndex + 1).toLowerCase();
	return imageFileTypes.includes(extension);
}

@customElement('umb-property-editor-ui-static-image-file-picker')
export class UmbPropertyEditorUIStaticImageFilePickerElement extends UmbPropertyEditorUIStaticFilePickerElement {
	#serializer = new UmbServerFilePathUniqueSerializer();
	#temporaryFileConfigRepository = new UmbTemporaryFileConfigRepository(this);

	@state()
	private _imageFileTypes?: Array<string>;

	constructor() {
		super();

		this.pickableFilter = (item) =>
			isPickableImageFile(this.#serializer.toServerPath(item.unique) ?? '', item.isFolder, this._imageFileTypes);

		this.#observeImageFileTypes();
	}

	async #observeImageFileTypes() {
		await this.#temporaryFileConfigRepository.initialized;
		if (!this.isConnected) return;
		this.observe(
			this.#temporaryFileConfigRepository.part('imageFileTypes'),
			(fileTypes) => {
				this._imageFileTypes = [
					...new Set([...(fileTypes ?? []).map((type) => type.toLowerCase()), ...ADDITIONAL_IMAGE_FILE_TYPES]),
				];
			},
			'_observeImageFileTypes',
		);
	}
}

export { UmbPropertyEditorUIStaticImageFilePickerElement as element };

export default UmbPropertyEditorUIStaticImageFilePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-static-image-file-picker': UmbPropertyEditorUIStaticImageFilePickerElement;
	}
}
