import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Tiny MCE',
	alias: 'Umbraco.TinyMCE',
	meta: {
		config: {
			properties: [
				{
					alias: 'toolbar',
					label: 'Toolbar',
					description: 'Pick the toolbar options that should be available when editing',
					propertyEditorUI: 'Umb.PropertyEditorUI.TinyMCE.ToolbarConfiguration',
				},
				{
					alias: 'stylesheets',
					label: 'Stylesheets',
					description: 'Pick the stylesheets whose editor styles should be available when editing',
					propertyEditorUI: 'Umb.PropertyEditorUI.TinyMCE.StylesheetsConfiguration',
				},
				{
					alias: 'dimensions',
					label: 'Dimensions',
					description: 'Set the editor dimensions',
					propertyEditorUI: 'Umb.PropertyEditorUI.TinyMCE.DimensionsConfiguration',
				},
				{
					alias: 'maxImageSize',
					label: 'Maximum size for inserted images',
					description: 'Maximum width or height - enter 0 to disable resizing',
					propertyEditorUI: 'Umb.PropertyEditorUI.TinyMCE.MaxImageSizeConfiguration',
				},
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
