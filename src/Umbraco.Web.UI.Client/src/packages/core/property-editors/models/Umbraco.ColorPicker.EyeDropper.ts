import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Eye Dropper Color Picker',
	alias: 'Umbraco.ColorPicker.EyeDropper',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.EyeDropper',
	},
};
