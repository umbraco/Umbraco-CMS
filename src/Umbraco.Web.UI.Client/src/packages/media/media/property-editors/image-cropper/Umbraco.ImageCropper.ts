import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Email Address',
	alias: 'Umbraco.ImageCropper',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.ImageCropper',
		settings: {
			properties: [
				{
					alias: 'crops',
					label: 'Crop options',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.ImageCropsConfiguration',
				},
			],
		},
	},
};
