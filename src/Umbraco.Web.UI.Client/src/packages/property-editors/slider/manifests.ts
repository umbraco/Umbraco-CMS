import { manifest as rangeSliderSchemaManifest } from './Umbraco.RangeSlider.js';
import { manifest as sliderSchemaManifest } from './Umbraco.Slider.js';
import { manifests as valueSummaryManifests } from './value-summary/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyValuePreset',
		forPropertyEditorSchemaAlias: 'Umbraco.Slider',
		alias: 'Umb.PropertyValuePreset.Slider',
		name: 'Property Editor Schema Slider Preset for Initial Values',
		api: () => import('./slider-property-value-preset.js'),
	},
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.Slider',
		name: 'Slider Property Editor UI',
		element: () => import('./property-editor-ui-slider.element.js'),
		meta: {
			label: 'Slider',
			propertyEditorSchemaAlias: 'Umbraco.Slider',
			icon: 'icon-navigation-horizontal',
			group: '#propertyEditorUIGroups_common',
			keywords: [
				'number',
				'range',
				'percentage',
				'rating',
				'level',
				'opacity',
				'scale',
				'volume',
				'score',
				'progress',
				'zoom',
			],
			supportsReadOnly: true,
			settings: {
				properties: [
					{
						alias: 'initVal1',
						label: 'Initial value',
						description: '',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Decimal',
						config: [{ alias: 'step', value: '0.00001' }],
					},
					{
						alias: 'step',
						label: 'Step increments',
						description: '',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Decimal',
						config: [{ alias: 'step', value: '0.00001' }],
					},
				],
				defaultData: [
					{
						alias: 'initVal1',
						value: 0.0,
					},
					{
						alias: 'step',
						value: 1.0,
					},
				],
			},
		},
	},
	{
		type: 'propertyValuePreset',
		forPropertyEditorSchemaAlias: 'Umbraco.RangeSlider',
		alias: 'Umb.PropertyValuePreset.RangeSlider',
		name: 'Property Editor Schema Range Slider Preset for Initial Values',
		api: () => import('./range-slider-property-value-preset.js'),
	},
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.RangeSlider',
		name: 'Range Slider Property Editor UI',
		element: () => import('./property-editor-ui-range-slider.element.js'),
		meta: {
			label: 'Range Slider',
			propertyEditorSchemaAlias: 'Umbraco.RangeSlider',
			icon: 'icon-navigation-horizontal',
			group: '#propertyEditorUIGroups_common',
			keywords: ['number', 'range', 'percentage', 'between', 'span', 'scale', 'from', 'to'],
			supportsReadOnly: true,
			settings: {
				properties: [
					{
						alias: 'initVal1',
						label: 'Initial low value',
						description: '',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Decimal',
						config: [{ alias: 'step', value: '0.00001' }],
					},
					{
						alias: 'initVal2',
						label: 'Initial high value',
						description: '',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Decimal',
						config: [{ alias: 'step', value: '0.00001' }],
					},
					{
						alias: 'step',
						label: 'Step increments',
						description: '',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Decimal',
						config: [{ alias: 'step', value: '0.00001' }],
					},
				],
				defaultData: [
					{
						alias: 'initVal1',
						value: 0.0,
					},
					{
						alias: 'initVal2',
						value: 0.0,
					},
					{
						alias: 'step',
						value: 1.0,
					},
				],
			},
		},
	},
	sliderSchemaManifest,
	rangeSliderSchemaManifest,
	...valueSummaryManifests,
];
