import { manifest as schemaManifest } from './Umbraco.ElementPicker.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.ElementPicker',
		name: 'Element Picker Property Editor UI',
		element: () => import('./property-editor-ui-element-picker.element.js'),
		meta: {
			label: 'Element Picker',
			propertyEditorSchemaAlias: 'Umbraco.ElementPicker',
			icon: 'icon-edit',
			group: 'common',
			supportsReadOnly: true,
		},
	},
	schemaManifest,
];
