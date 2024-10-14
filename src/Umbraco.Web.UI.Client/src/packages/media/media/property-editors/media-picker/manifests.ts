import { manifest as schemaManifest } from './Umbraco.MediaPicker.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.MediaPicker',
		name: 'Media Picker Property Editor UI',
		element: () => import('./property-editor-ui-media-picker.element.js'),
		meta: {
			label: 'Media Picker',
			propertyEditorSchemaAlias: 'Umbraco.MediaPicker3',
			icon: 'icon-picture',
			group: 'media',
			supportsReadOnly: true,
		},
	},
	schemaManifest,
];
