import { manifest as editorSchema } from './Umbraco.DateTimeUnspecified.js';

const editorUi: UmbExtensionManifest = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.DateTimePicker',
	name: 'Date Time Picker Property Editor UI',
	element: () => import('./property-editor-ui-date-time-picker.element.js'),
	meta: {
		label: 'Date Time (unspecified)',
		propertyEditorSchemaAlias: 'Umbraco.DateTimeUnspecified',
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
			],
			defaultData: [
				{
					alias: 'timeFormat',
					value: 'HH:mm',
				},
			],
		},
	},
};

const valuePresentation: UmbExtensionManifest = {
	type: 'propertyValuePresentation',
	alias: 'Umb.PropertyValuePresentation.DateTimePicker',
	name: 'Date Time Picker Property Value Presentation',
	element: () => import('./property-value-presentation-date-time-picker.element.js'),
	forPropertyEditorSchemaAlias: 'Umbraco.DateTimeUnspecified',
};

export const manifests: Array<UmbExtensionManifest> = [editorSchema, editorUi, valuePresentation];
