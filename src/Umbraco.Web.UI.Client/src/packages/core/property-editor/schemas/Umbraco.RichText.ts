import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Rich Text',
	alias: 'Umbraco.RichText',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.TinyMCE',
		settings: {
			properties: [
				{
					alias: 'mediaParentId',
					label: 'Image Upload Folder',
					description: 'Choose the upload location of pasted images',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.MediaPicker',
					config: [{ alias: 'validationLimit', value: { min: 0, max: 1 } }],
				},
				{
					alias: 'ignoreUserStartNodes',
					label: 'Ignore User Start Nodes',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
				{
					alias: 'blocks',
					label: 'Available Blocks',
					description: 'Define the available blocks.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.BlockRteTypeConfiguration',
				},
			],
		},
	},
};
