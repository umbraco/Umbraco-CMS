import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.DynamicRoot',
	name: 'Dynamic Root Property Editor UI',
	element: () => import('./property-editor-ui-dynamic-root.element.js'),
	meta: {
		label: 'Dynamic Root',
		icon: 'icon-tree',
		group: '#propertyEditorUIGroups_advanced',
	},
};
