import { manifest as schemaManifest } from './Umbraco.TimeOnly.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.TimeOnlyPicker',
		name: 'Time Only Picker Property Editor UI',
		element: () => import('./property-editor-ui-time-only-picker.element.js'),
		meta: {
			label: 'Time Only',
			propertyEditorSchemaAlias: 'Umbraco.TimeOnly',
			icon: 'icon-time',
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
	schemaManifest,
];
