import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.ElementPicker.AllowedElementTypes',
	name: 'Element Picker Allowed Element Types Property Editor UI',
	element: () =>
		import('./property-editor-ui-element-picker-allowed-element-types.element.js'),
	meta: {
		label: 'Element Picker Allowed Element Types',
		icon: 'icon-plugin',
		group: '#propertyEditorUIGroups_pickers',
	},
};
