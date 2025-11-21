import { manifest as editorSchema } from './Umbraco.TimeOnly.js';

const editorUi: UmbExtensionManifest = {
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
};

const valuePresentation: UmbExtensionManifest = {
	type: 'propertyValuePresentation',
	alias: 'Umb.PropertyValuePresentation.TimeOnlyPicker',
	name: 'Time Only Picker Property Value Presentation',
	element: () => import('./property-value-presentation-time-only-picker.element.js'),
	forPropertyEditorSchemaAlias: 'Umbraco.TimeOnly',
};

export const manifests: Array<UmbExtensionManifest> = [editorSchema, editorUi, valuePresentation];
