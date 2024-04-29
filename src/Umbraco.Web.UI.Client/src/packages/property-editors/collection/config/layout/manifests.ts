import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.Collection.LayoutConfiguration',
	name: 'Collection Column Configuration Property Editor UI',
	element: () => import('./layout-configuration.element.js'),
	meta: {
		label: 'Collection Layout Configuration',
		icon: 'icon-autofill',
		group: 'lists',
	},
};
