import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.TreePicker.Filter',
	name: 'Tree Picker Filter Property Editor UI',
	js: () => import('./property-editor-ui-tree-picker-filter.element.js'),
	meta: {
		label: 'Tree Picker Filter',
		icon: 'icon-page-add',
		group: 'pickers',
		propertyEditorSchemaAlias: '',
	},
};
