import { UMB_DOCUMENT_RECYCLE_BIN_ENTITY_TYPE, UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbDocumentRecycleBinTreeRepository } from './document-recycle-bin-tree.repository.js';
import { UmbDocumentRecycleBinTreeStore } from './document-recycle-bin-tree.store.js';
import { manifests as reloadTreeItemChildrenManifests } from './reload-tree-item-children/manifests.js';
import type {
	ManifestRepository,
	ManifestTree,
	ManifestTreeItem,
	ManifestTreeStore,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_RECYCLE_BIN_TREE_REPOSITORY_ALIAS = 'Umb.Repository.DocumentRecycleBin.Tree';
export const UMB_DOCUMENT_RECYCLE_BIN_TREE_STORE_ALIAS = 'Umb.Store.DocumentRecycleBin.Tree';
export const UMB_DOCUMENT_RECYCLE_BIN_TREE_ALIAS = 'Umb.Tree.DocumentRecycleBin';

const treeRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_RECYCLE_BIN_TREE_REPOSITORY_ALIAS,
	name: 'Document Recycle Bin Tree Repository',
	api: UmbDocumentRecycleBinTreeRepository,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: UMB_DOCUMENT_RECYCLE_BIN_TREE_STORE_ALIAS,
	name: 'Document Recycle Bin Tree Store',
	api: UmbDocumentRecycleBinTreeStore,
};

const tree: ManifestTree = {
	type: 'tree',
	alias: UMB_DOCUMENT_RECYCLE_BIN_TREE_ALIAS,
	name: 'Document Recycle Bin Tree',
	meta: {
		repositoryAlias: UMB_DOCUMENT_RECYCLE_BIN_TREE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'unique',
	alias: 'Umb.TreeItem.DocumentRecycleBin',
	name: 'DocumentRecycleBin Tree Item',
	meta: {
		entityTypes: [UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE, UMB_DOCUMENT_RECYCLE_BIN_ENTITY_TYPE],
	},
};

export const manifests = [treeRepository, treeStore, tree, treeItem, ...reloadTreeItemChildrenManifests];
