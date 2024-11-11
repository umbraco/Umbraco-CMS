import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const UMB_DOCUMENT_GRID_COLLECTION_VIEW_ALIAS = 'Umb.CollectionView.Document.Grid';
export const UMB_DOCUMENT_TABLE_COLLECTION_VIEW_ALIAS = 'Umb.CollectionView.Document.Table';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		alias: UMB_DOCUMENT_GRID_COLLECTION_VIEW_ALIAS,
		name: 'Document Grid Collection View',
		element: () => import('./grid/document-grid-collection-view.element.js'),
		weight: 200,
		meta: {
			label: 'Grid',
			icon: 'icon-grid',
			pathName: 'grid',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.Document',
			},
		],
	},
	{
		type: 'collectionView',
		alias: UMB_DOCUMENT_TABLE_COLLECTION_VIEW_ALIAS,
		name: 'Document Table Collection View',
		element: () => import('./table/document-table-collection-view.element.js'),
		weight: 300,
		meta: {
			label: 'Table',
			icon: 'icon-list',
			pathName: 'table',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.Document',
			},
		],
	},
];
