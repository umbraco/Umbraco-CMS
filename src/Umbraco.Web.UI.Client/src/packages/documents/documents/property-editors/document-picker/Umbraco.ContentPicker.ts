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
					label: 'Ignore user start nodes',
					description: 'Selecting this option allows a user to choose nodes that they normally dont have access to.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
			],
		},
	},
};
