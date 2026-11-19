import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.ValueListOptions',
	name: 'Value List Options Property Editor UI',
	element: () => import('./property-editor-ui-value-list-options.element.js'),
	meta: {
		label: 'Value List Options',
		icon: 'icon-ordered-list',
		group: '#propertyEditorUIGroups_lists',
		supportsReadOnly: true,
	},
};
