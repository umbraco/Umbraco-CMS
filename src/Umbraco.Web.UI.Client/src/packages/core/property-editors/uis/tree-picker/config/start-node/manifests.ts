import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUi.TreePicker.StartNode',
	name: 'Tree Picker Start Node Property Editor UI',
	loader: () => import('./property-editor-ui-tree-picker-start-node.element.js'),
	meta: {
		label: 'Tree Picker Start Node',
		icon: 'umb:page-add',
		group: 'pickers',
		propertyEditorModel: '',
	},
};
