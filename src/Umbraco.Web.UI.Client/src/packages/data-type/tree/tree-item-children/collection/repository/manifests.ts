import { UMB_DATA_TYPE_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DATA_TYPE_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS,
		name: 'Data Type Tree Item Children Collection Repository',
		api: () => import('./data-type-tree-item-children-collection.repository.js'),
	},
];
