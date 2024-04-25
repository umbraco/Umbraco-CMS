import { manifest as schemaManifest } from './Umbraco.ColorPicker.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
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
