import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Decimal',
	alias: 'Umbraco.Decimal',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.Decimal',
		settings: {
			properties: [
				{
					alias: 'min',
					label: 'Minimum',
					description: 'Enter the minimum amount of number to be entered',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Number',
				},
				{
					alias: 'max',
					label: 'Maximum',
					description: 'Enter the minimum amount of number to be entered',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Number',
				},
				{
					alias: 'step',
					label: 'Step size',
					description: 'Enter the intervals amount between each step of number to be entered',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Number',
				},
			],
		},
	},
};
