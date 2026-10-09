import { UMB_MEMBER_GROUP_COLLECTION_ALIAS } from '../constants.js';
import { UMB_MEMBER_GROUP_TABLE_COLLECTION_VIEW_ALIAS } from './constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		kind: 'table',
		alias: UMB_MEMBER_GROUP_TABLE_COLLECTION_VIEW_ALIAS,
		name: 'Member Group Table Collection View',
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_MEMBER_GROUP_COLLECTION_ALIAS,
			},
		],
	},
];
