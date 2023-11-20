import { UmbDocumentTypeTreeRepository } from './document-type-tree.repository.js';
import { UmbDocumentTypeTreeStore } from './document-type.tree.store.js';
import type {
	ManifestRepository,
	ManifestTree,
	ManifestTreeItem,
	ManifestTreeStore,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_TYPE_TREE_REPOSITORY_ALIAS = 'Umb.Repository.DocumentType.Tree';
export const UMB_DOCUMENT_TYPE_TREE_STORE_ALIAS = 'Umb.Store.DocumentType.Tree';
export const UMB_DOCUMENT_TYPE_TREE_ALIAS = 'Umb.Tree.DocumentType';

const treeRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_TYPE_TREE_REPOSITORY_ALIAS,
	name: 'Document Type Tree Repository',
	api: UmbDocumentTypeTreeRepository,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: UMB_DOCUMENT_TYPE_TREE_STORE_ALIAS,
	name: 'Document Type Tree Store',
	api: UmbDocumentTypeTreeStore,
};

const tree: ManifestTree = {
	type: 'tree',
	alias: UMB_DOCUMENT_TYPE_TREE_ALIAS,
	name: 'Document Type Tree',
	meta: {
		repositoryAlias: UMB_DOCUMENT_TYPE_TREE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'entity',
	alias: 'Umb.TreeItem.DocumentType',
	name: 'Document Type Tree Item',
	meta: {
		entityTypes: ['document-type-root', 'document-type'],
	},
};

export const manifests = [treeRepository, treeStore, tree, treeItem];
