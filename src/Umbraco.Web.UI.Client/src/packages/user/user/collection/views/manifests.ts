import { UMB_USER_COLLECTION_ALIAS } from '../constants.js';
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
				match: UMB_USER_COLLECTION_ALIAS,
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
				match: UMB_USER_COLLECTION_ALIAS,
			},
		],
	},
	{
		type: 'collectionView',
		kind: 'table',
		alias: 'USER TEST',
		name: 'User Table Collection View',
		meta: {
			label: 'Table KIND',
			icon: 'icon-table',
			pathName: 'table2',
			columns: [
				{
					field: 'userGroupIds',
					label: 'User Groups',
				},
				{
					field: 'lastLoginDate',
					label: 'Last login',
				},
				{
					field: 'state',
					label: 'Status',
				},
			],
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_USER_COLLECTION_ALIAS,
			},
		],
	},
];
