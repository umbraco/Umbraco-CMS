import { manifest as schemaManifest } from './Umbraco.DateTime.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.DatePicker',
		name: 'Date Picker Property Editor UI',
		element: () => import('./property-editor-ui-date-picker.element.js'),
		meta: {
			label: 'Date Picker',
			propertyEditorSchemaAlias: 'Umbraco.DateTime',
			icon: 'icon-time',
			group: 'pickers',
			supportsReadOnly: true,
			settings: {
				properties: [
					{
						alias: 'format',
						label: 'Date format',
						description: 'If left empty then the format is YYYY-MM-DD',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox',
					},
				],
				defaultData: [
					{
						alias: 'format',
						value: 'YYYY-MM-DD HH:mm:ss',
					},
				],
			},
		},
	},
	schemaManifest,
];
