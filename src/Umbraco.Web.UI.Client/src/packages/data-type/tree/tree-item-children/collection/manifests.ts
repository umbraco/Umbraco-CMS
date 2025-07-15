import { manifests as viewManifests } from './views/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as actionManifests } from './action/manifests.js';
import { UMB_DATA_TYPE_TREE_ITEM_CHILDREN_COLLECTION_ALIAS } from './constants.js';
import { UMB_DATA_TYPE_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS } from './repository/index.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collection',
		kind: 'default',
		alias: UMB_DATA_TYPE_TREE_ITEM_CHILDREN_COLLECTION_ALIAS,
		name: 'Data Type Tree Item Children Collection',
		meta: {
			repositoryAlias: UMB_DATA_TYPE_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS,
		},
	},
	...actionManifests,
	...viewManifests,
	...repositoryManifests,
];
