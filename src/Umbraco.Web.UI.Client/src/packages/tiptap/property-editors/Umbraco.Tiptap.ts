import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Rich Text Tiptap',
	alias: 'Umbraco.Tiptap',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.Tiptap',
		settings: {
			properties: [],
		},
	},
};
