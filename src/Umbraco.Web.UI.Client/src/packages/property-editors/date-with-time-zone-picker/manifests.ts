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
						alias: 'timeZonesToEdit',
						label: 'Time Zones to Edit',
						description: 'Select the time zones that the editor should be able to select from. If left empty, only the local time zone will be displayed.',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.TimeZonePicker',
						config: [],
					},
					{
						alias: 'timeZonesToDisplay',
						label: 'Time Zones to Display',
						description: 'Select the time zones that the user should be able to see the time in.',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.CheckBoxList',
						config: [
							{
								alias: "items",
								value: ["Local", "UTC"]
							}
						]
					}
				],
				defaultData: [
					{
						alias: 'timeZonesToEdit',
						value: [],
					},
					{
						alias: 'timeZonesToDisplay',
						value: [],
					}
				],
			},
		},
	},
	schemaManifest,
];
