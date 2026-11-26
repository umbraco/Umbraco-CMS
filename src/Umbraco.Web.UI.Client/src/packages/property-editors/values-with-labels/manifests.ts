import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.ValuesWithLabels',
	name: 'Values With Labels Property Editor UI',
	element: () => import('./property-editor-ui-values-with-labels.element.js'),
	meta: {
		label: 'Values With Labels',
		icon: 'icon-ordered-list',
		group: '#propertyEditorUIGroups_lists',
		supportsReadOnly: true,
	},
};
