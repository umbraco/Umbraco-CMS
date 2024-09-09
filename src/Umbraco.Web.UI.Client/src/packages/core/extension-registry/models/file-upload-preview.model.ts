import type { UmbFileUploadPreviewElement } from '../interfaces/file-upload-preview.interface.js';
import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestFileUploadPreview extends ManifestElement<UmbFileUploadPreviewElement> {
	type: 'fileUploadPreview';
	/**
	 * Array of the mime types that are supported by this extension.
	 * @example ["image/png", "image/jpeg", "image/*"]
	 */
	forMimeTypes: Array<string>;
}
