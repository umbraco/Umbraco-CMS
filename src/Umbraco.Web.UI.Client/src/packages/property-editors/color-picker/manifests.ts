import { manifest as editorSchema } from './Umbraco.ColorPicker.js';
import { UMB_COLOR_PICKER_PROPERTY_EDITOR_UI_ALIAS } from './constants.js';

const editorUi: UmbExtensionManifest = {
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
};

const valuePresentation: UmbExtensionManifest = {
	type: 'propertyValuePresentation',
	alias: 'Umb.PropertyValuePresentation.ColorPicker',
	name: 'Color Picker Property Value Presentation',
	element: () => import('./property-value-presentation-color-picker.element.js'),
	forPropertyEditorSchemaAlias: 'Umbraco.ColorPicker',
};

export const manifests: Array<UmbExtensionManifest> = [editorSchema, editorUi, valuePresentation];
