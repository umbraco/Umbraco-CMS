import { UMB_PARTIAL_VIEW_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_PARTIAL_VIEW_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS,
		name: 'Partial View Tree Item Children Collection Repository',
		api: () => import('./partial-view-tree-item-children-collection.repository.js'),
	},
];
