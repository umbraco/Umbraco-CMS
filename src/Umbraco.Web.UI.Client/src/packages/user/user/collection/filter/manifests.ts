import { UMB_USER_COLLECTION_ALIAS } from '../constants.js';
import { manifests as UserGroupManifests } from './user-group/manifests.js';
import { manifests as UserStateManifests } from './user-state/manifests.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionTextFilter',
		kind: 'default',
		alias: 'Umb.CollectionTextFilter.User',
		name: 'User Collection Text Filter',
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_USER_COLLECTION_ALIAS,
			},
		],
	},
	...UserGroupManifests,
	...UserStateManifests,
];
