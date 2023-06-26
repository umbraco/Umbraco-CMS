import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Rich Text',
	alias: 'Umbraco.TinyMCE',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.TinyMCE',
		settings: {
			properties: [
				{
					alias: 'mediaParentId',
					label: 'Image Upload Folder',
					description: 'Choose the upload location of pasted images',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.TreePicker',
				},
				{
					alias: 'ignoreUserStartNodes',
					label: 'Ignore User Start Nodes',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
			],
		},
	},
};
