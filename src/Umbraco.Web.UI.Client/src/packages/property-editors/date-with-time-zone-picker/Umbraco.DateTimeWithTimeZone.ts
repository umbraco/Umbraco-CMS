import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Date Time with Time Zone',
	alias: 'Umbraco.DateTimeWithTimeZone',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.DateWithTimeZonePicker',
		settings: {
			properties: [
				{
					alias: 'format',
					label: '#timeZonePicker_config_format',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.RadioButtonList',
					config: [
						{
							alias: 'items',
							value: [
								{ name: '#timeZonePicker_config_format_datetime', value: 'date-time' },
								{ name: '#timeZonePicker_config_format_dateOnly', value: 'date-only' },
								{ name: '#timeZonePicker_config_format_timeOnly', value: 'time-only' },
							],
						},
					],
				},
				{
					alias: 'timeFormat',
					label: '#timeZonePicker_config_timeFormat',
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
					label: '#timeZonePicker_config_timeZones',
					description: '{#timeZonePicker_config_timeZones_description}',
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
};
