import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUI = {
	type: 'propertyEditorUI',
	alias: 'Umb.PropertyEditorUi.CollectionView.ColumnConfiguration',
	name: 'Collection View Column Configuration Property Editor UI',
	loader: () => import('./property-editor-ui-collection-view-column-configuration.element.js'),
	meta: {
		label: 'Collection View Column Configuration',
		propertyEditorModel: '',
		icon: 'umb:autofill',
		group: 'lists',
	},
};
