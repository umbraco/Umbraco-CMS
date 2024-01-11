import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.TreePicker.SourceTypePicker',
	name: 'Tree Picker Source Type Picker Property Editor UI',
	js: () => import('./property-editor-ui-tree-picker-source-type-picker.element.js'),
	meta: {
		label: 'Tree Picker Source Type Picker',
		icon: 'icon-page-add',
		group: 'pickers',
		propertyEditorSchemaAlias: '',
	},
};
