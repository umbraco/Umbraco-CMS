import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Text Box',
	alias: 'Umbraco.TextBox',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox',
		settings: {
			properties: [
				{
					alias: 'maxChars',
					label: 'Maximum allowed characters',
					description: 'If empty, 512 character limit',
					propertyEditorUi: 'Umb.PropertyEditorUi.Number',
				},
			],
			defaultData: [
				{
					alias: 'maxChars',
					value: 512,
				},
			],
		},
	},
};
