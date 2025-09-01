import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Date Time 2',
	alias: 'Umbraco.DateTime2',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.DateTimePicker',
		settings: {
			properties: [
				{
					alias: 'format',
					label: '#dateTimePicker_config_format',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.RadioButtonList',
					config: [
						{
							alias: 'items',
							value: [
								{ name: '#dateTimePicker_config_format_datetime', value: 'date-time' },
								{ name: '#dateTimePicker_config_format_dateOnly', value: 'date-only' },
								{ name: '#dateTimePicker_config_format_timeOnly', value: 'time-only' },
							],
						},
					],
				},
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
