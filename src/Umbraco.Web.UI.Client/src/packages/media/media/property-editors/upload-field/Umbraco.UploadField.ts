import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'File upload',
	alias: 'Umbraco.UploadField',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.UploadField',
		settings: {
			properties: [
				{
					alias: 'fileExtensions',
					label: 'Accepted file extensions',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.AcceptedUploadTypes',
				},
			],
		},
	},
};
