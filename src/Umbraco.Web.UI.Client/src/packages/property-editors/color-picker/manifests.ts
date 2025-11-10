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
	{
		type: 'propertyValuePresentation',
		alias: 'Umb.PropertyValuePresentation.ColorPicker',
		name: 'Color Picker Property Value Presentation',
		element: () => import('./property-value-presentation-color-picker.js'),
		propertyEditorAlias: 'Umbraco.ColorPicker',
	},
	schemaManifest,
];
