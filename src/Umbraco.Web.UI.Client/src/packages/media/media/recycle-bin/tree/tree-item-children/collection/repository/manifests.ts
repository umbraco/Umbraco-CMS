import { UMB_MEDIA_RECYCLE_BIN_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEDIA_RECYCLE_BIN_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS,
		name: 'Media Recycle Bin Tree Item Children Collection Repository',
		api: () => import('./media-recycle-bin-tree-item-children-collection.repository.js'),
	},
];
