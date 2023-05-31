import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'User Picker',
	alias: 'Umbraco.UserPicker',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUI.UserPicker',
	},
};
