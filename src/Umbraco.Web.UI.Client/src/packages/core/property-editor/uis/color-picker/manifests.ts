import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.ColorPicker',
	name: 'Color Picker Property Editor UI',
	js: () => import('./property-editor-ui-color-picker.element.js'),
	meta: {
		label: 'Color Picker',
		propertyEditorSchemaAlias: 'Umbraco.ColorPicker',
		icon: 'icon-colorpicker',
		group: 'pickers',
	},
};
