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
					description:
						'Insert one extension per line, for example `jpg`.\n\nYou can also use mime types, for example `image/*` or `application/pdf`.',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.AcceptedUploadTypes',
				},
			],
		},
	},
};
