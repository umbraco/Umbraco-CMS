import {
	UMB_DOCUMENT_TYPE_TREE_ITEM_CHILDREN_COLLECTION_ALIAS,
	UMB_DOCUMENT_TYPE_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS,
} from './constants.js';
import { manifests as viewManifests } from './views/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collection',
		kind: 'default',
		alias: UMB_DOCUMENT_TYPE_TREE_ITEM_CHILDREN_COLLECTION_ALIAS,
		name: 'Document Type Tree Item Children Collection',
		meta: {
			repositoryAlias: UMB_DOCUMENT_TYPE_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'repository',
		alias: UMB_DOCUMENT_TYPE_TREE_ITEM_CHILDREN_COLLECTION_REPOSITORY_ALIAS,
		name: 'Document Type Tree Item Children Collection Repository',
		api: () => import('./document-type-tree-item-children-collection.repository.js'),
	},
	...viewManifests,
];
