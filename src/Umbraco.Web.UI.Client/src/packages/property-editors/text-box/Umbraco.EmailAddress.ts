import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Email Address',
	alias: 'Umbraco.EmailAddress',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.EmailAddress',
	},
};
