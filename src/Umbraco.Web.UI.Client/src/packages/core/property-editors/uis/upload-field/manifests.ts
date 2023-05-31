import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUi.UploadField',
	name: 'Upload Field Property Editor UI',
	loader: () => import('./property-editor-ui-upload-field.element.js'),
	meta: {
		label: 'Upload Field',
		propertyEditorModel: 'Umbraco.UploadField',
		icon: 'umb:download-alt',
		group: 'common',
	},
};
