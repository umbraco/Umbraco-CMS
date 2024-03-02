import {
	UMB_PARTIAL_VIEW_ENTITY_TYPE,
	UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE,
	UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE,
} from '../entity.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { manifests as reloadTreeItemChildrenManifest } from './reload-tree-item-children/manifests.js';
import type {
	ManifestRepository,
	ManifestTree,
	ManifestTreeItem,
	ManifestTreeStore,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_PARTIAL_VIEW_TREE_REPOSITORY_ALIAS = 'Umb.Repository.PartialView.Tree';
export const UMB_PARTIAL_VIEW_TREE_STORE_ALIAS = 'Umb.Store.PartialView.Tree';
export const UMB_PARTIAL_VIEW_TREE_ALIAS = 'Umb.Tree.PartialView';

const treeRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_PARTIAL_VIEW_TREE_REPOSITORY_ALIAS,
	name: 'Partial View Tree Repository',
	api: () => import('./partial-view-tree.repository.js'),
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: UMB_PARTIAL_VIEW_TREE_STORE_ALIAS,
	name: 'Partial View Tree Store',
	api: () => import('./partial-view-tree.store.js'),
};

const tree: ManifestTree = {
	type: 'tree',
	kind: 'default',
	alias: UMB_PARTIAL_VIEW_TREE_ALIAS,
	name: 'Partial View Tree',
	meta: {
		repositoryAlias: UMB_PARTIAL_VIEW_TREE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'default',
	alias: 'Umb.TreeItem.PartialView',
	name: 'Partial View Tree Item',
	meta: {
		entityTypes: [UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE, UMB_PARTIAL_VIEW_ENTITY_TYPE, UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE],
	},
};

export const manifests = [
	treeRepository,
	treeStore,
	tree,
	treeItem,
	...folderManifests,
	...reloadTreeItemChildrenManifest,
];
