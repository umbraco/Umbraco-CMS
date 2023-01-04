import type { ManifestPropertyEditorModel } from '@umbraco-cms/models';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Multiple Text String',
	alias: 'Umbraco.MultipleTextString',
	meta: {
		config: {
			properties: [
				{
					alias: 'minNumber',
					label: 'Minimum',
					description: 'Enter the minimum amount of text boxes to be displayed',
					propertyEditorUI: 'Umb.PropertyEditorUI.Number',
				},
				{
					alias: 'maxNumber',
					label: 'Maximum',
					description: 'Enter the maximum amount of text boxes to be displayed, enter 0 for unlimited',
					propertyEditorUI: 'Umb.PropertyEditorUI.Number',
				},
			],
			defaultData: [
				{
					alias: 'minNumber',
					value: 0,
				},
				{
					alias: 'maxNumber',
					value: 0,
				},
			],
		},
	},
};
