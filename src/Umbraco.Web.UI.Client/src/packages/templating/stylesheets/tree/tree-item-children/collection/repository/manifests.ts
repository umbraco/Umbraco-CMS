import { UMB_STYLESHEET_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_STYLESHEET_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS,
		name: 'Stylesheet Tree Item Children Collection Repository',
		api: () => import('./stylesheet-tree-item-children-collection.repository.js'),
	},
];
