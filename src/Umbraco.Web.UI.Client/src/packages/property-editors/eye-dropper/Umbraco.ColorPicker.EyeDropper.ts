import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorSchema = {
	type: 'propertyEditorSchema',
	name: 'Eye Dropper Color Picker',
	alias: 'Umbraco.ColorPicker.EyeDropper',
	meta: {
		defaultPropertyEditorUiAlias: 'Umb.PropertyEditorUi.EyeDropper',
	},
};
