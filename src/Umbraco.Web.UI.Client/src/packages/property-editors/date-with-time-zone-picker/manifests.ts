import { manifest as schemaManifest } from './Umbraco.DateTimeWithTimeZone.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.DateWithTimeZonePicker',
		name: 'Date With Timezone Picker Property Editor UI',
		element: () => import('./property-editor-ui-date-with-time-zone-picker.element.js'),
		meta: {
			label: 'Date With Time Zone Picker',
			propertyEditorSchemaAlias: 'Umbraco.DateTimeWithTimeZone',
			icon: 'icon-time',
			group: 'pickers',
			supportsReadOnly: true,
			settings: {
				properties: [
					{
						alias: 'format',
						label: 'Format',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.RadioButtonList',
						config: [
							{
								alias: 'items',
								value: [
									{ name: 'Date and time', value: 'date-time' },
									{ name: 'Date only', value: 'date-only' },
									{ name: 'Time only', value: 'time-only' },
								],
							},
						],
					},
					{
						alias: 'timeFormat',
						label: 'Time format',
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
						label: 'Time zones',
						description: "Only applicable when format is set to 'Date and time'.",
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.TimeZonePicker',
						config: [],
					},
				],
				defaultData: [
					{
						alias: 'format',
						value: 'date-time',
					},
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
