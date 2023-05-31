import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Email Address',
	alias: 'Umbraco.EmailAddress',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUI.EmailAddress',
	},
};
