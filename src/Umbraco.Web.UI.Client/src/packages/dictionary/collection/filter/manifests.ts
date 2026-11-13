import { UMB_DICTIONARY_COLLECTION_ALIAS } from '../constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionTextFilter',
		kind: 'default',
		alias: 'Umb.CollectionTextFilter.Dictionary',
		name: 'Dictionary Collection Text Filter',
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_DICTIONARY_COLLECTION_ALIAS,
			},
		],
	},
];
