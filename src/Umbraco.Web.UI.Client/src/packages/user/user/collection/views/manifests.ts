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
			icon: 'icon-table',
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
		kind: 'card',
		alias: UMB_COLLECTION_VIEW_USER_GRID,
		name: 'User Grid Collection View',
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.User',
			},
		],
	},
];
