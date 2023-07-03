import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Icon Picker',
	alias: 'Umbraco.IconPicker',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.IconPicker',
	},
};
