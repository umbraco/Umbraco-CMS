import { manifest as trueFalseSchemaManifest } from './Umbraco.TrueFalse.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyValuePreset',
		forPropertyEditorSchemaAlias: 'Umbraco.TrueFalse',
		alias: 'Umb.PropertyValuePreset.TrueFalse',
		name: 'Property Editor Schema True/False Preset for Initial State',
		api: () => import('./true-false-property-value-preset.js'),
	},
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.Toggle',
		name: 'Toggle Property Editor UI',
		element: () => import('./property-editor-ui-toggle.element.js'),
		meta: {
			label: 'Toggle',
			propertyEditorSchemaAlias: 'Umbraco.TrueFalse',
			icon: 'icon-checkbox',
			group: 'common',
			supportsReadOnly: true,
			settings: {
				properties: [
					{
						alias: 'default',
						label: 'Preset value',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
						config: [
							{
								alias: 'ariaLabel',
								value: 'toggle for the initial state of this data type',
							},
						],
					},
					{
						alias: 'showLabels',
						label: 'Show on/off labels',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
						config: [
							{
								alias: 'ariaLabel',
								value: 'toggle for weather if label should be displayed',
							},
						],
					},
					{
						alias: 'labelOn',
						label: 'Label On',
						description: 'Displays text when enabled.',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox',
					},
					{
						alias: 'labelOff',
						label: 'Label Off',
						description: 'Displays text when disabled.',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox',
					},
					{
						alias: 'ariaLabel',
						label: 'Screen Reader Label',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox',
					},
				],
			},
		},
	},
	trueFalseSchemaManifest,
];
