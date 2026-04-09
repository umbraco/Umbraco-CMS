import { UMB_USER_COLLECTION_ALIAS } from '../constants.js';
import { UMB_COLLECTION_VIEW_USER_GRID, UMB_COLLECTION_VIEW_USER_TABLE } from './constants.js';
import { UMB_USER_STATE_VALUE_TYPE, UMB_USER_LAST_LOGIN_VALUE_TYPE } from '../../value-summary/constants.js';
import { UMB_USER_GROUPS_VALUE_TYPE } from '../../../user-group/value-summary/constants.js';
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
		alias: 'Umb.CollectionView.User.Table.Kind',
		name: 'User Table Kind Collection View',
		meta: {
			label: 'Table',
			icon: 'icon-table',
			pathName: 'table2',
			columns: [
				{
					field: 'userGroupUniques',
					label: 'User Groups',
					valueType: UMB_USER_GROUPS_VALUE_TYPE,
				},
				{
					field: 'lastLoginDate',
					label: 'Last login',
					valueType: UMB_USER_LAST_LOGIN_VALUE_TYPE,
				},
				{
					field: 'state',
					label: 'Status',
					valueType: UMB_USER_STATE_VALUE_TYPE,
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
