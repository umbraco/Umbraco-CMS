import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.Collection.OrderBy',
	name: 'Collection View Column Configuration Property Editor UI',
	element: () => import('./order-by.element.js'),
	meta: {
		label: 'Collection View Order By',
		icon: 'icon-autofill',
		group: 'lists',
	},
};
