import { manifest as schemaManifest } from './Umbraco.UploadField.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
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
	},
	schemaManifest,
];
