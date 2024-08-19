import { manifest as schemaManifest } from './Umbraco.ImageCropper.js';
import type { ManifestPropertyEditorUi, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.ImageCropper',
	name: 'Image Cropper Property Editor UI',
	element: () => import('./property-editor-ui-image-cropper.element.js'),
	meta: {
		label: 'Image Cropper',
		icon: 'icon-crop',
		group: 'media',
		propertyEditorSchemaAlias: 'Umbraco.ImageCropper',
	},
};

export const manifests: Array<ManifestTypes> = [manifest, schemaManifest];
