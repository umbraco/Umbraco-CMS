import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Content Picker',
	alias: 'Umbraco.ContentPicker',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.DocumentPicker',
		settings: {
			properties: [
				{
					alias: 'ignoreUserStartNodes',
					label: 'Ignore user access',
					description: 'allow users to choose items exceeding their access.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
			],
		},
	},
};
