import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Textarea',
	alias: 'Umbraco.TextArea',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.TextArea',
		settings: {
			properties: [
				{
					alias: 'maxChars',
					label: 'Maximum allowed characters',
					description: 'If empty - no character limit',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Number',
				},
			],
		},
	},
};
