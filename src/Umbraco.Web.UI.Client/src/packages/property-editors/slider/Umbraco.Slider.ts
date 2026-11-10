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
			],
			defaultData: [{ alias: 'validationRange', value: { min: 0.0, max: 100.0 } }],
		},
	},
};
