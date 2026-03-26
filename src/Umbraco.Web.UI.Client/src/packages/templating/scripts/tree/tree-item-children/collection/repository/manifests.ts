import { UMB_SCRIPT_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_SCRIPT_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS,
		name: 'Script Tree Item Children Collection Repository',
		api: () => import('./script-tree-item-children-collection.repository.js'),
	},
];
