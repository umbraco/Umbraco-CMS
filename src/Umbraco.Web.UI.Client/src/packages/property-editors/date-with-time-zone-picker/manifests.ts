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
						alias: 'timeZones',
						label: 'Time Zones',
						description: 'Select the time zones that the user should be able to select from.',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.TimeZonePicker',
						config: [],
					},
				],
				defaultData: [
					{
						alias: 'timeZones',
						value: ['UTC'],
					},
				],
			},
		},
	},
	schemaManifest,
];
