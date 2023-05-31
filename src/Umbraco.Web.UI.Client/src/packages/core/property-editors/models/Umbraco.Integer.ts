import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Decimal',
	alias: 'Umbraco.Integer',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.Integer',
		settings: {
			properties: [
				{
					alias: 'min',
					label: 'Minimum',
					description: 'Enter the minimum amount of number to be entered',
					propertyEditorUi: 'Umb.PropertyEditorUi.Number',
				},
				{
					alias: 'max',
					label: 'Maximum',
					description: 'Enter the minimum amount of number to be entered',
					propertyEditorUi: 'Umb.PropertyEditorUi.Number',
				},
				{
					alias: 'step',
					label: 'Step size',
					description: 'Enter the intervals amount between each step of number to be entered',
					propertyEditorUi: 'Umb.PropertyEditorUi.Number',
				},
			],
		},
	},
};
