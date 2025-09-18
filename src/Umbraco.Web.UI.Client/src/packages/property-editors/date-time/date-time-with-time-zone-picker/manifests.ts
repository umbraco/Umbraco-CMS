import { manifest as schemaManifest } from './Umbraco.DateTimeWithTimeZone.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.DateTimeWithTimeZonePicker',
		name: 'Date Time with Time Zone Picker Property Editor UI',
		element: () => import('./property-editor-ui-date-time-with-time-zone-picker.element.js'),
		meta: {
			label: 'Date Time (with time zone)',
			propertyEditorSchemaAlias: 'Umbraco.DateTimeWithTimeZone',
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
					{
						alias: 'timeZones',
						label: '#dateTimePicker_config_timeZones',
						description: '{#dateTimePicker_config_timeZones_description}',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.TimeZonePicker',
						config: [],
					},
				],
				defaultData: [
					{
						alias: 'timeFormat',
						value: 'HH:mm',
					},
					{
						alias: 'timeZones',
						value: {
							mode: 'all',
							timeZones: [],
						},
					},
				],
			},
		},
	},
	schemaManifest,
];
