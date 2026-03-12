import { manifests as memberTypeFilterManifests } from './member-type/manifests.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionTextFilter',
		kind: 'default',
		alias: 'Umb.CollectionTextFilter.Member',
		name: 'Member Collection Text Filter',
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.Member',
			},
		],
	},
	...memberTypeFilterManifests,
];
