import { manifests as viewManifests } from './views/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_MEDIA_TYPE_TREE_ITEM_CHILDREN_COLLECTION_ALIAS } from './constants.js';
import { UMB_MEDIA_TYPE_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS } from './repository/index.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collection',
		kind: 'default',
		alias: UMB_MEDIA_TYPE_TREE_ITEM_CHILDREN_COLLECTION_ALIAS,
		name: 'Media Type Tree Item Children Collection',
		meta: {
			repositoryAlias: UMB_MEDIA_TYPE_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS,
		},
	},
	...viewManifests,
	...repositoryManifests,
];
