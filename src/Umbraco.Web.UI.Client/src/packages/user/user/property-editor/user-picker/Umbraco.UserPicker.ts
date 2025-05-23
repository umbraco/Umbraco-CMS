import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'User Picker',
	alias: 'Umbraco.UserPicker',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.UserPicker',
	},
};
