import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.UploadField',
	name: 'Upload Field Property Editor UI',
	loader: () => import('./property-editor-ui-upload-field.element'),
	meta: {
		label: 'Upload Field',
		propertyEditorModel: 'Umbraco.UploadField',
		icon: 'umb:download-alt',
		group: 'common',
	},
};
