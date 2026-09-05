import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Single URL Picker',
	alias: 'Umbraco.UrlPicker.Single',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.UrlPicker.Single',
		settings: {
			properties: [
				{
					alias: 'ignoreUserStartNodes',
					label: 'Ignore user start nodes',
					description: "Selecting this option allows a user to choose nodes that they normally don't have access to.",
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
			],
		},
	},
};
