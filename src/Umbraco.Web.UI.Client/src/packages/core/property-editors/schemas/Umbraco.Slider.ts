import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/extension-registry';

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
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Number',
				},
				{
					alias: 'maxVal',
					label: 'Maximum value',
					description: '',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Number',
				},
			],
			defaultData: [
				{
					alias: 'minVal',
					value: 0,
				},
				{
					alias: 'maxVal',
					value: 0,
				},
			],
		},
	},
};
