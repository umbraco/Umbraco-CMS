import { manifest as schemaManifest } from './Umbraco.ColorPicker.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.ColorPicker',
		name: 'Color Picker Property Editor UI',
		element: () => import('./property-editor-ui-color-picker.element.js'),
		meta: {
			label: 'Color Picker',
			propertyEditorSchemaAlias: 'Umbraco.ColorPicker',
			icon: 'icon-colorpicker',
			group: 'pickers',
		},
	},
	schemaManifest,
];
