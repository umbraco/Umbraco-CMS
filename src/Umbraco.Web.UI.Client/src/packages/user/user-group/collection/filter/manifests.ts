import { UMB_USER_GROUP_COLLECTION_ALIAS } from '../constants.js';
import { UMB_COLLECTION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionTextFilter',
		kind: 'default',
		alias: 'Umb.Collection.TextFilter.UserGroup',
		name: 'User Group Collection Text Filter',
		conditions: [
			{
				alias: UMB_COLLECTION_CONDITION_ALIAS,
				match: UMB_USER_GROUP_COLLECTION_ALIAS,
			},
		],
	},
];
