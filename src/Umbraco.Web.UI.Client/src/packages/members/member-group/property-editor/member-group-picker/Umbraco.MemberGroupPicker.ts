import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Member Group Picker',
	alias: 'Umbraco.MemberGroupPicker',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.MemberGroupPicker',
	},
};
