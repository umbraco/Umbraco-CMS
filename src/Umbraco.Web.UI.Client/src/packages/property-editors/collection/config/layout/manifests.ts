import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.Collection.LayoutConfiguration',
	name: 'Collection View Column Configuration Property Editor UI',
	element: () => import('./layout-configuration.element.js'),
	meta: {
		label: 'Collection View Layout Configuration',
		icon: 'icon-autofill',
		group: 'lists',
	},
};
