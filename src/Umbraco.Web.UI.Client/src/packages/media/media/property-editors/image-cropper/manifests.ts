import { manifest as schemaManifest } from './Umbraco.ImageCropper.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
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
	},
	schemaManifest,
];
