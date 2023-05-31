import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorModel = {
	type: 'propertyEditorModel',
	name: 'Icon Picker',
	alias: 'Umbraco.IconPicker',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUI.IconPicker',
	},
};
