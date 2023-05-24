import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.ColorPicker',
	name: 'Color Picker Property Editor UI',
	loader: () => import('./property-editor-ui-color-picker.element.js'),
	meta: {
		label: 'Color Picker',
		propertyEditorModel: 'Umbraco.ColorPicker',
		icon: 'umb:colorpicker',
		group: 'pickers',
	},
};
