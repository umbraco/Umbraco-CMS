import type { ManifestPropertyEditorUI } from '@umbraco-cms/models';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.TinyMCE.Config',
	name: 'Tiny MCE Configuration Property Editor UI',
	loader: () => import('./property-editor-ui-tiny-mce-configuration.element'),
	meta: {
		label: 'Rich Text Editor Configuration',
		propertyEditorModel: 'Umbraco.TinyMCE.Configuration',
		icon: 'umb:autofill',
		group: 'common',
	},
};
