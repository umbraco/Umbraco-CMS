import { UMB_DICTIONARY_TABLE_COLLECTION_VIEW_ALIAS } from './constants.js';
import { UMB_COLLECTION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionView',
		alias: UMB_DICTIONARY_TABLE_COLLECTION_VIEW_ALIAS,
		name: 'Dictionary Table Collection View',
		element: () => import('./table/dictionary-table-collection-view.element.js'),
		meta: {
			label: 'Table',
			icon: 'icon-table',
			pathName: 'table',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_CONDITION_ALIAS,
				match: 'Umb.Collection.Dictionary',
			},
		],
	},
];
