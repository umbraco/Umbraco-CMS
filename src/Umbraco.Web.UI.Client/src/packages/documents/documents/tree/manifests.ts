import { UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbDocumentTreeRepository } from './document-tree.repository.js';
import { UmbDocumentTreeStore } from './document-tree.store.js';
import { manifests as reloadTreeItemChildrenManifests } from './reload-tree-item-children/manifests.js';
import type {
	ManifestRepository,
	ManifestTree,
	ManifestTreeItem,
	ManifestTreeStore,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_TREE_REPOSITORY_ALIAS = 'Umb.Repository.Document.Tree';
export const UMB_DOCUMENT_TREE_STORE_ALIAS = 'Umb.Store.Document.Tree';
export const UMB_DOCUMENT_TREE_ALIAS = 'Umb.Tree.Document';

const treeRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_TREE_REPOSITORY_ALIAS,
	name: 'Document Tree Repository',
	api: UmbDocumentTreeRepository,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: UMB_DOCUMENT_TREE_STORE_ALIAS,
	name: 'Document Tree Store',
	api: UmbDocumentTreeStore,
};

const tree: ManifestTree = {
	type: 'tree',
	alias: UMB_DOCUMENT_TREE_ALIAS,
	name: 'Document Tree',
	meta: {
		repositoryAlias: UMB_DOCUMENT_TREE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	alias: 'Umb.TreeItem.Document',
	name: 'Document Tree Item',
	js: () => import('./tree-item/document-tree-item.element.js'),
	meta: {
		entityTypes: [UMB_DOCUMENT_ROOT_ENTITY_TYPE, UMB_DOCUMENT_ENTITY_TYPE],
	},
};

export const manifests = [treeRepository, treeStore, tree, treeItem, ...reloadTreeItemChildrenManifests];
