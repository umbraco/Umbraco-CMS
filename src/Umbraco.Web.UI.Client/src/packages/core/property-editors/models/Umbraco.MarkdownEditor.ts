import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Markdown Editor',
	alias: 'Umbraco.MarkdownEditor',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.MarkdownEditor',
	},
};
