import {
	UMB_SEARCH_COLLECTION_REPOSITORY_ALIAS,
	UMB_SEARCH_COLLECTION_VIEW_ALIAS,
	UMB_SEARCH_ROOT_COLLECTION_ALIAS,
} from '../constants.js';
import { UMB_SEARCH_LEGACY_ROOT_COLLECTION_ALIAS, loadWithDeprecationWarning } from '../legacy-aliases.js';
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
		type: 'collection',
		kind: 'default',
		name: 'Umbraco Search - Root Collection (deprecated alias)',
		alias: UMB_SEARCH_LEGACY_ROOT_COLLECTION_ALIAS,
		api: () =>
			loadWithDeprecationWarning(
				UMB_SEARCH_LEGACY_ROOT_COLLECTION_ALIAS,
				UMB_SEARCH_ROOT_COLLECTION_ALIAS,
				() => import('./search-collection.context.js'),
			),
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
