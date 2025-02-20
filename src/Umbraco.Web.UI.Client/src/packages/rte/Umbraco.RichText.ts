import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Rich Text',
	alias: 'Umbraco.RichText',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.Tiptap',
		settings: {
			properties: [
				{
					alias: 'blocks',
					label: 'Available Blocks',
					description: 'Define the available blocks.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.BlockRteTypeConfiguration',
					weight: 80,
				},
				{
					alias: 'mediaParentId',
					label: 'Image Upload Folder',
					description: 'Choose the upload location of pasted images',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.MediaEntityPicker',
					config: [{ alias: 'validationLimit', value: { min: 0, max: 1 } }],
					weight: 90,
				},
				{
					alias: 'ignoreUserStartNodes',
					label: 'Ignore User Start Nodes',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
					weight: 100,
				},
			],
		},
	},
};
