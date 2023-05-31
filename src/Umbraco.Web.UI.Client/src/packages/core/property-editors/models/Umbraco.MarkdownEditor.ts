import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Markdown Editor',
	alias: 'Umbraco.MarkdownEditor',
	meta: {
		defaultUI: 'Umb.PropertyEditorUI.MarkdownEditor',
	},
};
