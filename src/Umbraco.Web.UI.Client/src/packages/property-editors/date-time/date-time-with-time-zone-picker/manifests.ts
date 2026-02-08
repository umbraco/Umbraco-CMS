import { manifest as editorSchema } from './Umbraco.DateTimeWithTimeZone.js';

const editorUi: UmbExtensionManifest = {
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
};

const valuePresentation: UmbExtensionManifest = {
	type: 'propertyValuePresentation',
	alias: 'Umb.PropertyValuePresentation.DateTimeWithTimeZonePicker',
	name: 'Date Time With Time Zone Picker Property Value Presentation',
	element: () => import('./property-value-presentation-date-time-with-time-zone-picker.element.js'),
	forPropertyEditorSchemaAlias: 'Umbraco.DateTimeWithTimeZone',
};

export const manifests = [editorSchema, editorUi, valuePresentation];
