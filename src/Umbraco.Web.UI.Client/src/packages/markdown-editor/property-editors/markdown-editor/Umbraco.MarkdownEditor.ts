import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Markdown Editor',
	alias: 'Umbraco.MarkdownEditor',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.MarkdownEditor',
	},
};
