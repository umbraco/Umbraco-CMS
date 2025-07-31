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
						label: 'Time zones',
						description: 'Select the time zones that the editor should be able to select from. If left empty, only the local time zone will be displayed.',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.TimeZonePicker',
						config: [],
					},
					{
						alias: 'displayLocalTime',
						label: 'Display local time',
						description: "Selecting this option will display the user's local time below the date input.",
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
						config: []
					}
				],
				defaultData: [
					{
						alias: 'timeZones',
						value: [],
					},
					{
						alias: 'displayLocalTime',
						value: false,
					}
				],
			},
		},
	},
	schemaManifest,
];
