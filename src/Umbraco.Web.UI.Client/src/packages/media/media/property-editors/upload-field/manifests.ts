import { manifest as schemaManifest } from './Umbraco.UploadField.js';
import type {
	ManifestFileUploadPreview,
	ManifestPropertyEditorUi,
	ManifestTypes,
} from '@umbraco-cms/backoffice/extension-registry';

const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.UploadField',
	name: 'Upload Field Property Editor UI',
	element: () => import('./property-editor-ui-upload-field.element.js'),
	meta: {
		label: 'Upload Field',
		propertyEditorSchemaAlias: 'Umbraco.UploadField',
		icon: 'icon-download-alt',
		group: 'media',
	},
};

/** Testing */
const previews: ManifestFileUploadPreview = {
	type: 'fileUploadPreview',
	alias: 'My PDF Showcase',
	name: 'PDF Showcase',
	element: () => import('./test.element.js'),
	forMimeTypes: ['application/pdf'],
};

export const manifests: Array<ManifestTypes> = [manifest, schemaManifest, previews];
