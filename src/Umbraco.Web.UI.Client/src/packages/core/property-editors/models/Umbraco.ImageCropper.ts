import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Email Address',
	alias: 'Umbraco.ImageCropper',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.ImageCropper',
		settings: {
			properties: [
				{
					alias: 'crops',
					label: 'Define Crops',
					propertyEditorUI: 'Umb.PropertyEditorUi.ImageCropsConfiguration',
				},
			],
		},
	},
};
