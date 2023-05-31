import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUi.CollectionView.OrderBy',
	name: 'Collection View Column Configuration Property Editor UI',
	loader: () => import('./property-editor-ui-collection-view-order-by.element.js'),
	meta: {
		label: 'Collection View Order By',
		propertyEditorModel: '',
		icon: 'umb:autofill',
		group: 'lists',
	},
};
