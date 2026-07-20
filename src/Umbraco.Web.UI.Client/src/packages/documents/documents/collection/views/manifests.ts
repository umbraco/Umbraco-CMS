import { UMB_DOCUMENT_GRID_COLLECTION_VIEW_ALIAS, UMB_DOCUMENT_TABLE_COLLECTION_VIEW_ALIAS } from './constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		kind: 'card',
		alias: UMB_DOCUMENT_GRID_COLLECTION_VIEW_ALIAS,
		name: 'Document Grid Collection View',
		weight: 200,
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
			icon: 'icon-table',
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
