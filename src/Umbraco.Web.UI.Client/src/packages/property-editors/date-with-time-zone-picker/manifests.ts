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
						description: "Select the date format.",
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.RadioButtonList',
						config: [
							{
								alias: 'items',
								value: [
									{ name: 'Date only', value: 'date-only' },
									{ name: 'Date and time', value: 'date-time' },
									{ name: 'Date and time with time zone', value: 'date-time-timezone' },
								],
							},

						],
					},
					{
						alias: 'timeZones',
						label: 'Time zones',
						description: "Select the time zones to be available in the picker. Only relevant when 'Date and time with time zone' is selected as format.",
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.TimeZonePicker',
						config: [],
					}
				],
				defaultData: [
					{
						alias: 'format',
						value: 'date-time-timezone',
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
