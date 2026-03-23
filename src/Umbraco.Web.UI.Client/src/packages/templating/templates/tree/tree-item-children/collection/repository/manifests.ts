import { UMB_TEMPLATE_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_TEMPLATE_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS,
		name: 'Template Tree Item Children Collection Repository',
		api: () => import('./template-tree-item-children-collection.repository.js'),
	},
];
