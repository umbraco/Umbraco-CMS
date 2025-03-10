import { manifest as sliderSchemaManifest } from './Umbraco.Slider.js';
import { UMB_SLIDER_PROPERTY_EDITOR_SCHEMA_ALIAS, UMB_SLIDER_PROPERTY_EDITOR_UI_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyValuePreset',
		forPropertyEditorSchemaAlias: UMB_SLIDER_PROPERTY_EDITOR_SCHEMA_ALIAS,
		forPropertyEditorUiAlias: UMB_SLIDER_PROPERTY_EDITOR_UI_ALIAS,
		alias: 'Umb.PropertyValuePreset.Slider',
		name: 'Property Editor Schema Slider Preset for Initial Values',
		api: () => import('./slider-property-value-preset.js'),
	},
	{
		type: 'propertyEditorUi',
		alias: UMB_SLIDER_PROPERTY_EDITOR_UI_ALIAS,
		name: 'Slider Property Editor UI',
		element: () => import('./property-editor-ui-slider.element.js'),
		meta: {
			label: 'Slider',
			propertyEditorSchemaAlias: UMB_SLIDER_PROPERTY_EDITOR_SCHEMA_ALIAS,
			icon: 'icon-navigation-horizontal',
			group: 'common',
			supportsReadOnly: true,
			settings: {
				properties: [
					{
						alias: 'enableRange',
						label: 'Enable range',
						description: '',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
					},
					{
						alias: 'initVal1',
						label: 'Initial value',
						description: '',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Integer',
					},
					{
						alias: 'initVal2',
						label: 'Initial value 2',
						description: 'Used when range is enabled',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Integer',
					},
					{
						alias: 'step',
						label: 'Step increments',
						description: '',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Integer',
					},
				],
				defaultData: [
					{
						alias: 'initVal1',
						value: 0,
					},
					{
						alias: 'initVal2',
						value: 0,
					},
					{
						alias: 'step',
						value: 1,
					},
				],
			},
		},
	},
	sliderSchemaManifest,
];
