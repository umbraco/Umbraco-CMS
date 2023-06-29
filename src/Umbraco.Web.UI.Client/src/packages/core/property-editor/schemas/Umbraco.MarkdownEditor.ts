import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Markdown Editor',
	alias: 'Umbraco.MarkdownEditor',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.MarkdownEditor',
	},
};
