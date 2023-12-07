import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.UploadField',
	name: 'Upload Field Property Editor UI',
	js: () => import('./property-editor-ui-upload-field.element.js'),
	meta: {
		label: 'Upload Field',
		propertyEditorSchemaAlias: 'Umbraco.UploadField',
		icon: 'icon-download-alt',
		group: 'common',
	},
};
