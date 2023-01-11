import type { ManifestPropertyEditorModel } from '@umbraco-cms/models';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Email Address',
	alias: 'Umbraco.ImageCropper',
	meta: {
		config: {
			properties: [
				{
					alias: 'crops',
					label: 'Define Crops',
					propertyEditorUI: 'Umb.PropertyEditorUI.ImageCropsConfiguration',
				},
			],
		},
	},
};
