import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
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
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Integer',
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
