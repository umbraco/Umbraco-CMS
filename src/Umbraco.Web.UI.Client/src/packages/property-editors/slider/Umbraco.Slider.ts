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
			],
			defaultData: [
				{ alias: 'minVal', value: 0.0 },
				{ alias: 'maxVal', value: 100.0 },
			],
		},
	},
};
