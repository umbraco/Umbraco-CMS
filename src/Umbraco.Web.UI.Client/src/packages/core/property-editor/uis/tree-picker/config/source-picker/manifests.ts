import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.TreePicker.SourcePicker',
	name: 'Tree Picker Source Picker Property Editor UI',
	element: () => import('./property-editor-ui-tree-picker-source-picker.element.js'),
	meta: {
		label: 'Tree Picker Source Picker',
		icon: 'icon-page-add',
		group: 'pickers',
		propertyEditorSchemaAlias: '',
	},
};
