import { UMB_COLOR_PICKER_PROPERTY_EDITOR_UI_ALIAS } from './constants.js';
import { manifest as schemaManifest } from './Umbraco.ColorPicker.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: UMB_COLOR_PICKER_PROPERTY_EDITOR_UI_ALIAS,
		name: 'Color Picker Property Editor UI',
		element: () => import('./property-editor-ui-color-picker.element.js'),
		meta: {
			label: 'Color Picker',
			propertyEditorSchemaAlias: 'Umbraco.ColorPicker',
			icon: 'icon-colorpicker',
			group: 'pickers',
			supportsReadOnly: true,
		},
	},
	schemaManifest,
];
