import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as viewManifests } from './views/manifests.js';
import { UMB_MEDIA_RECYCLE_BIN_TREE_ITEM_CHILDREN_COLLECTION_ALIAS } from './constants.js';
import { UMB_MEDIA_RECYCLE_BIN_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS } from './repository/index.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collection',
		kind: 'default',
		alias: UMB_MEDIA_RECYCLE_BIN_TREE_ITEM_CHILDREN_COLLECTION_ALIAS,
		name: 'Media Recycle Bin Tree Item Children Collection',
		meta: {
			repositoryAlias: UMB_MEDIA_RECYCLE_BIN_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS,
		},
	},
	...repositoryManifests,
	...viewManifests,
];
