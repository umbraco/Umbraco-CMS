import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Slider',
	alias: 'Umbraco.Slider',
	meta: {
		config: {
			properties: [
				{
					alias: 'minVal',
					label: 'Minimum value',
					description: '',
					propertyEditorUI: 'Umb.PropertyEditorUI.Number',
				},
				{
					alias: 'maxVal',
					label: 'Maximum value',
					description: '',
					propertyEditorUI: 'Umb.PropertyEditorUI.Number',
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
