import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.Collection.OrderBy',
	name: 'Collection Column Configuration Property Editor UI',
	element: () => import('./order-by.element.js'),
	meta: {
		label: 'Collection Order By',
		icon: 'icon-autofill',
		group: 'lists',
	},
};
