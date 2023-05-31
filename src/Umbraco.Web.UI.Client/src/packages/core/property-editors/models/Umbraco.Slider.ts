import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
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
					propertyEditorUI: 'Umb.PropertyEditorUi.Number',
				},
				{
					alias: 'maxVal',
					label: 'Maximum value',
					description: '',
					propertyEditorUI: 'Umb.PropertyEditorUi.Number',
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
