import { UMB_USER_COLLECTION_ALIAS } from '../constants.js';
import { UMB_USER_STATE_VALUE_TYPE } from '../../value-summary/constants.js';
import { UMB_USER_GROUP_REFERENCES_VALUE_TYPE } from '../../../user-group/value-summary/constants.js';
import { UMB_COLLECTION_VIEW_USER_GRID, UMB_COLLECTION_VIEW_USER_TABLE } from './constants.js';
import { UMB_DATE_TIME_VALUE_TYPE } from '@umbraco-cms/backoffice/value-type';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		kind: 'table',
		alias: UMB_COLLECTION_VIEW_USER_TABLE,
		name: 'User Table Collection View',
		weight: 700,
		meta: {
			columns: [
				{
					field: 'userGroupUniques',
					label: 'User Groups',
					valueType: UMB_USER_GROUP_REFERENCES_VALUE_TYPE,
				},
				{
					field: 'lastLoginDate',
					label: 'Last login',
					valueType: UMB_DATE_TIME_VALUE_TYPE,
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
];
