import type { UmbFileUploadPreviewElement } from '../interfaces/file-upload-preview.interface.js';
import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestFileUploadPreview extends ManifestElement<UmbFileUploadPreviewElement> {
	type: 'fileUploadPreview';
	forMimeTypes?: Array<string>;
}
