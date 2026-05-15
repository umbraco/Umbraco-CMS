import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import { UMB_EXTENSION_COLLECTION_ALIAS } from '../constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		kind: 'table',
		alias: 'Umb.CollectionView.Extension.Table',
		name: 'Extension Table Collection View',
		meta: {
			columns: [
				{
					field: 'manifest.type',
					label: '#general_type',
				},
				{
					field: 'manifest.weight',
					label: '#general_weight',
				},
			],
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_EXTENSION_COLLECTION_ALIAS,
			},
		],
	},
];
