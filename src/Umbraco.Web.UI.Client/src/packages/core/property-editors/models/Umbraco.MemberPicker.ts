import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Member Picker',
	alias: 'Umbraco.MemberPicker',
	meta: {
		defaultUI: 'Umb.PropertyEditorUI.MemberPicker',
	},
};
