import { UMB_MEDIA_TYPE_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEDIA_TYPE_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS,
		name: 'Media Type Tree Item Children Collection Repository',
		api: () => import('./media-type-tree-item-children-collection.repository.js'),
	},
];
