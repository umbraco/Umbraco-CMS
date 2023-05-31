import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Tiny MCE',
	alias: 'Umbraco.TinyMCE',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUI.TinyMCE',
		settings: {
			properties: [
				{
					alias: 'mediaParentId',
					label: 'Image Upload Folder',
					propertyEditorUI: 'Umb.PropertyEditorUI.TreePicker',
				},
				{
					alias: 'ignoreUserStartNodes',
					label: 'Ignore User Start Nodes',
					propertyEditorUI: 'Umb.PropertyEditorUI.Toggle',
				},
			],
		},
	},
};
