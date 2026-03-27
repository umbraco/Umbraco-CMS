import { UMB_DOCUMENT_BLUEPRINT_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_BLUEPRINT_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS,
		name: 'Document Blueprint Tree Item Children Collection Repository',
		api: () => import('./document-blueprint-tree-item-children-collection.repository.js'),
	},
];
