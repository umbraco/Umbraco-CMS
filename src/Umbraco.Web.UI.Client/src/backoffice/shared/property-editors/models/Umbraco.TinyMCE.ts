import type { ManifestPropertyEditorModel } from '@umbraco-cms/models';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Tiny MCE',
	alias: 'Umbraco.TinyMCE',
	meta: {
		config: {
			properties: [
				{
					alias: 'mediaParentId',
					label: 'Image Upload Folder',
					description: 'Choose the upload location of pasted images',
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
