import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUI.CollectionView.OrderBy',
	name: 'Collection View Column Configuration Property Editor UI',
	loader: () => import('./property-editor-ui-collection-view-order-by.element'),
	meta: {
		label: 'Collection View Order By',
		propertyEditorModel: '',
		icon: 'umb:autofill',
		group: 'lists',
	},
};
