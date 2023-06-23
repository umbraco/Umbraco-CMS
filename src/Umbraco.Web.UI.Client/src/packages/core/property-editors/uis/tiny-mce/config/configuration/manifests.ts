import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.TinyMCE.Config',
	name: 'Tiny MCE Configuration Property Editor UI',
	loader: () => import('./property-editor-ui-tiny-mce-configuration.element.js'),
	meta: {
		label: 'Rich Text Editor Configuration',
		propertyEditorSchemaAlias: 'Umbraco.TinyMCE.Configuration',
		icon: 'umb:autofill',
		group: 'common',
	},
};
