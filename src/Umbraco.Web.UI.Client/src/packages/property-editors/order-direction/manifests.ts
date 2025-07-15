import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.OrderDirection',
	name: 'Order Direction Property Editor UI',
	element: () => import('./property-editor-ui-order-direction.element.js'),
	meta: {
		label: 'Order Direction',
		icon: 'icon-autofill',
		group: 'common',
	},
};
