import { manifests as actionManifests } from './action/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as viewManifests } from './views/manifests.js';
import { UMB_STYLESHEET_TREE_ITEM_CHILDREN_COLLECTION_ALIAS } from './constants.js';
import { UMB_STYLESHEET_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS } from './repository/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collection',
		kind: 'default',
		alias: UMB_STYLESHEET_TREE_ITEM_CHILDREN_COLLECTION_ALIAS,
		name: 'Stylesheet Tree Item Children Collection',
		meta: {
			repositoryAlias: UMB_STYLESHEET_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS,
		},
	},
	...actionManifests,
	...repositoryManifests,
	...viewManifests,
];
