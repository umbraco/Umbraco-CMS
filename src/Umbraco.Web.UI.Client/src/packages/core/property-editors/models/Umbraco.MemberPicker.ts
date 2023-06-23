import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Member Picker',
	alias: 'Umbraco.MemberPicker',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.MemberPicker',
	},
};
