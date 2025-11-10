import { manifest as schemaManifest } from './Umbraco.DateTimeUnspecified.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.DateTimePicker',
		name: 'Date Time Picker Property Editor UI',
		element: () => import('./property-editor-ui-date-time-picker.element.js'),
		meta: {
			label: 'Date Time (unspecified)',
			propertyEditorSchemaAlias: 'Umbraco.DateTimeUnspecified',
			icon: 'icon-calendar-alt',
			group: 'date',
			supportsReadOnly: true,
			settings: {
				properties: [
					{
						alias: 'timeFormat',
						label: '#dateTimePicker_config_timeFormat',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.RadioButtonList',
						config: [
							{
								alias: 'items',
								value: [
									{ name: 'HH:mm', value: 'HH:mm' },
									{ name: 'HH:mm:ss', value: 'HH:mm:ss' },
								],
							},
						],
					},
				],
				defaultData: [
					{
						alias: 'timeFormat',
						value: 'HH:mm',
					},
				],
			},
		},
	},
	{
		type: 'propertyValuePresentation',
		alias: 'Umb.PropertyValuePresentation.DateTimePicker',
		name: 'Date Time Picker Property Value Presentation',
		element: () => import('./property-value-presentation-date-time-picker.js'),
		propertyEditorAlias: 'Umbraco.DateTimeUnspecified',
	},
	schemaManifest,
];
