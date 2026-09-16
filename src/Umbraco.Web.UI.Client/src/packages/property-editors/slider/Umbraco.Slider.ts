import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Slider',
	alias: 'Umbraco.Slider',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.Slider',
		settings: {
			properties: [
				{
					alias: 'validationRange',
					label: 'Value range',
					description: 'Set the minimum and maximum value of the slider.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.NumberRange',
					config: [{ alias: 'step', value: 0.00001 }],
				},
				{
					alias: 'minimumRange',
					label: 'Minimum range',
					description:
						'Minimum difference between the low and high values when range is enabled. Set to 0 to allow equal values.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Decimal',
					config: [{ alias: 'step', value: '0.00001' }],
				},
			],
			defaultData: [
				{ alias: 'validationRange', value: { min: 0.0, max: 100.0 } },
				{ alias: 'minimumRange', value: 0.0 },
			],
		},
	},
};
