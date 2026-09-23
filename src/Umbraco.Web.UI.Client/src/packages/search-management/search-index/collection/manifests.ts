import {
	UMB_SEARCH_COLLECTION_REPOSITORY_ALIAS,
	UMB_SEARCH_COLLECTION_VIEW_ALIAS,
	UMB_SEARCH_ROOT_COLLECTION_ALIAS,
} from '../constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as entityActionManifests } from './entity-actions/manifests.js';
import { manifests as collectionActionManifests } from './collection-actions/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...repositoryManifests,
	...entityActionManifests,
	...collectionActionManifests,
	{
		type: 'collection',
		kind: 'default',
		name: 'Umbraco Search - Root Collection',
		alias: UMB_SEARCH_ROOT_COLLECTION_ALIAS,
		api: () => import('./search-collection.context.js'),
		meta: {
			repositoryAlias: UMB_SEARCH_COLLECTION_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'collectionView',
		name: 'Umbraco Search - Root Collection View',
		alias: UMB_SEARCH_COLLECTION_VIEW_ALIAS,
		element: () => import('./search-root-collection-view.element.js'),
		meta: {
			label: '#searchManagement_treeHeader',
			icon: 'icon-search',
			pathName: 'table',
		},
		conditions: [
			{
				alias: 'Umb.Condition.CollectionAlias',
				match: UMB_SEARCH_ROOT_COLLECTION_ALIAS,
			},
		],
	},
];
