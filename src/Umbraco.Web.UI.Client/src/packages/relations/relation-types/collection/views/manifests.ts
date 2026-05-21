import { UMB_RELATION_TYPE_COLLECTION_ALIAS } from '../constants.js';
import { UMB_COLLECTION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		kind: 'table',
		alias: 'Umb.CollectionView.RelationType.Table',
		name: 'Relation Type Table Collection View',
		conditions: [
			{
				alias: UMB_COLLECTION_CONDITION_ALIAS,
				match: UMB_RELATION_TYPE_COLLECTION_ALIAS,
			},
		],
	},
];
