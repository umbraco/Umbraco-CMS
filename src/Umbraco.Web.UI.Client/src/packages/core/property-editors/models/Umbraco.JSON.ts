import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'JSON model',
	alias: 'Umbraco.JSON',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.JSON',
	},
};
