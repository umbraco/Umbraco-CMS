import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
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
