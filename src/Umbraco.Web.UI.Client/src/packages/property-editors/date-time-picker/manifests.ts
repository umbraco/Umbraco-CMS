import { manifest as schemaManifest } from './Umbraco.DateTime2.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.DateTimePicker',
		name: 'Date Time Picker Property Editor UI',
		element: () => import('./property-editor-ui-date-time-picker.element.js'),
		meta: {
			label: 'Date Time Picker',
			propertyEditorSchemaAlias: 'Umbraco.DateTime2',
			icon: 'icon-time',
			group: 'pickers',
			supportsReadOnly: true,
		},
	},
	schemaManifest,
];
