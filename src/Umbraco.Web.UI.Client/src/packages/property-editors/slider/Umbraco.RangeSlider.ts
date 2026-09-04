import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Range Slider',
	alias: 'Umbraco.RangeSlider',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.RangeSlider',
		settings: {
			properties: [
				{
					alias: 'minVal',
					label: 'Minimum value',
					description: '',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Decimal',
					config: [{ alias: 'step', value: '0.00001' }],
				},
				{
					alias: 'maxVal',
					label: 'Maximum value',
					description: '',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Decimal',
					config: [{ alias: 'step', value: '0.00001' }],
				},
				{
					alias: 'minimumRange',
					label: 'Minimum range',
					description: 'Minimum difference between the low and high values. Set to 0 to allow equal values.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Decimal',
					config: [{ alias: 'step', value: '0.00001' }],
				},
			],
			defaultData: [
				{ alias: 'minVal', value: 0.0 },
				{ alias: 'maxVal', value: 100.0 },
				{ alias: 'minimumRange', value: 0.0 },
			],
		},
	},
};
