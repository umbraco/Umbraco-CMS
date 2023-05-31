import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

// TODO: We won't include momentjs anymore so we need to find a way to handle date formats
export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'File upload',
	alias: 'Umbraco.UploadField',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.UploadField',
		settings: {
			properties: [
				{
					alias: 'fileExtensions',
					label: 'Accepted file extensions',
					propertyEditorUi: 'Umb.PropertyEditorUi.MultipleTextString',
				},
			],
		},
	},
};
