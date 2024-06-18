import { UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE } from '../entity.js';
import {
	UMB_DOCUMENT_RECYCLE_BIN_TREE_ALIAS,
	UMB_DOCUMENT_RECYCLE_BIN_TREE_REPOSITORY_ALIAS,
	UMB_DOCUMENT_RECYCLE_BIN_TREE_STORE_ALIAS,
} from './constants.js';
import { UmbDocumentRecycleBinTreeStore } from './document-recycle-bin-tree.store.js';
import { manifests as reloadTreeItemChildrenManifests } from './reload-tree-item-children/manifests.js';
import type {
	ManifestRepository,
	ManifestTree,
	ManifestTreeItem,
	ManifestTreeStore,
	ManifestTypes,
} from '@umbraco-cms/backoffice/extension-registry';

const treeRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_RECYCLE_BIN_TREE_REPOSITORY_ALIAS,
	name: 'Document Recycle Bin Tree Repository',
	api: () => import('./document-recycle-bin-tree.repository.js'),
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: UMB_DOCUMENT_RECYCLE_BIN_TREE_STORE_ALIAS,
	name: 'Document Recycle Bin Tree Store',
	api: () => import('./document-recycle-bin-tree.store.js'),
};

const tree: ManifestTree = {
	type: 'tree',
	kind: 'default',
	alias: UMB_DOCUMENT_RECYCLE_BIN_TREE_ALIAS,
	name: 'Document Recycle Bin Tree',
	meta: {
		repositoryAlias: UMB_DOCUMENT_RECYCLE_BIN_TREE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'default',
	alias: 'Umb.TreeItem.Document.RecycleBin',
	name: 'Document Recycle Bin Tree Item',
	forEntityTypes: [UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE],
};

export const manifests: Array<ManifestTypes> = [
	treeRepository,
	treeStore,
	tree,
	treeItem,
	...reloadTreeItemChildrenManifests,
];
