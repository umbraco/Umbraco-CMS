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
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Integer',
					config: [
						{ alias: 'min', value: 1 },
						{ alias: 'max', value: 512 },
						{ alias: 'placeholder', value: '512' },
					],
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
