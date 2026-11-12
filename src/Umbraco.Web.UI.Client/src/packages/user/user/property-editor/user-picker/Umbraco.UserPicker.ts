import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Single User Picker',
	alias: 'Umbraco.UserPicker',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.UserPicker',
	},
};
