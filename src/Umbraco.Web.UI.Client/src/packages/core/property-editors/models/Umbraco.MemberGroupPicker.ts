import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Member Group Picker',
	alias: 'Umbraco.MemberGroupPicker',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUI.MemberGroupPicker',
	},
};
