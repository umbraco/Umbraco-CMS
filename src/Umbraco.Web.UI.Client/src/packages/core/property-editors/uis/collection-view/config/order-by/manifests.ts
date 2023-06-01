import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.CollectionView.OrderBy',
	name: 'Collection View Column Configuration Property Editor UI',
	loader: () => import('./property-editor-ui-collection-view-order-by.element.js'),
	meta: {
		label: 'Collection View Order By',
		propertyEditorAlias: '',
		icon: 'umb:autofill',
		group: 'lists',
	},
};
