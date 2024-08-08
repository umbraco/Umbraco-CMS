import {
	UMB_STATIC_FILE_ENTITY_TYPE,
	UMB_STATIC_FILE_FOLDER_ENTITY_TYPE,
	UMB_STATIC_FILE_ROOT_ENTITY_TYPE,
} from '../entity.js';
import {
	UMB_STATIC_FILE_TREE_ALIAS,
	UMB_STATIC_FILE_TREE_ITEM_ALIAS,
	UMB_STATIC_FILE_TREE_REPOSITORY_ALIAS,
	UMB_STATIC_FILE_TREE_STORE_ALIAS,
} from './constants.js';
import type {
	ManifestRepository,
	ManifestTree,
	ManifestTreeStore,
	ManifestTreeItem,
	ManifestTypes,
} from '@umbraco-cms/backoffice/extension-registry';

const treeRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_STATIC_FILE_TREE_REPOSITORY_ALIAS,
	name: 'Static File Tree Repository',
	api: () => import('./static-file-tree.repository.js'),
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: UMB_STATIC_FILE_TREE_STORE_ALIAS,
	name: 'Static File Tree Store',
	api: () => import('./static-file-tree.store.js'),
};

const tree: ManifestTree = {
	type: 'tree',
	kind: 'default',
	alias: UMB_STATIC_FILE_TREE_ALIAS,
	name: 'Static File Tree',
	meta: {
		repositoryAlias: UMB_STATIC_FILE_TREE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'default',
	alias: UMB_STATIC_FILE_TREE_ITEM_ALIAS,
	name: 'Static File Tree Item',
	forEntityTypes: [UMB_STATIC_FILE_ENTITY_TYPE, UMB_STATIC_FILE_ROOT_ENTITY_TYPE, UMB_STATIC_FILE_FOLDER_ENTITY_TYPE],
};

export const manifests: Array<ManifestTypes> = [treeRepository, treeStore, tree, treeItem];
