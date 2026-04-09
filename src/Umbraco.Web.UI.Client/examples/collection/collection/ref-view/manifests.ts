import { EXAMPLE_COLLECTION_ALIAS } from '../constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		kind: 'ref',
		alias: 'Example.CollectionView.Ref',
		name: 'Example Ref Collection View',
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: EXAMPLE_COLLECTION_ALIAS,
			},
		],
	},
];
