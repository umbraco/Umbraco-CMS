import type { ManifestPropertyEditorUI } from '@umbraco-cms/models';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.TreePicker',
	name: 'Tree Picker Property Editor UI',
	loader: () => import('./property-editor-ui-tree-picker.element'),
	meta: {
		label: 'Tree Picker',
		icon: 'umb:page-add',
		group: 'pickers',
		propertyEditorModel: 'Umbraco.JSON',
	},
};
