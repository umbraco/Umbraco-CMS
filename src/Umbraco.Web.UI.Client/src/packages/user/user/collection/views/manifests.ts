import { UMB_COLLECTION_VIEW_USER_GRID, UMB_COLLECTION_VIEW_USER_TABLE } from './constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		alias: UMB_COLLECTION_VIEW_USER_TABLE,
		name: 'User Table Collection View',
		element: () => import('./table/user-table-collection-view.element.js'),
		meta: {
			label: 'Table',
			icon: 'icon-list',
			pathName: 'table',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.User',
			},
		],
	},
	{
		type: 'collectionView',
		alias: UMB_COLLECTION_VIEW_USER_GRID,
		name: 'User Grid Collection View',
		element: () => import('./grid/user-grid-collection-view.element.js'),
		weight: 200,
		meta: {
			label: 'Grid',
			icon: 'icon-grid',
			pathName: 'grid',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.User',
			},
		],
	},
];
