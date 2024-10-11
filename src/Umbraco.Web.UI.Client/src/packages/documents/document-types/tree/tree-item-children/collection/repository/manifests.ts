import { UMB_DOCUMENT_TYPE_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_TYPE_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS,
		name: 'Document Type Tree Item Children Collection Repository',
		api: () => import('./document-type-tree-item-children-collection.repository.js'),
	},
];
