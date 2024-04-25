import { manifests as folderManifests } from './folder/manifests.js';
import { manifests as reloadManifests } from './reload-tree-item-children/manifests.js';
import {
	UMB_DATA_TYPE_TREE_ALIAS,
	UMB_DATA_TYPE_TREE_REPOSITORY_ALIAS,
	UMB_DATA_TYPE_TREE_STORE_ALIAS,
} from './constants.js';
import type {
	ManifestRepository,
	ManifestTree,
	ManifestTreeItem,
	ManifestTreeStore,
	ManifestTypes,
} from '@umbraco-cms/backoffice/extension-registry';

const treeRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DATA_TYPE_TREE_REPOSITORY_ALIAS,
	name: 'Data Type Tree Repository',
	api: () => import('./data-type-tree.repository.js'),
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: UMB_DATA_TYPE_TREE_STORE_ALIAS,
	name: 'Data Type Tree Store',
	api: () => import('./data-type-tree.store.js'),
};

const tree: ManifestTree = {
	type: 'tree',
	kind: 'default',
	alias: UMB_DATA_TYPE_TREE_ALIAS,
	name: 'Data Types Tree',
	meta: {
		repositoryAlias: UMB_DATA_TYPE_TREE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'default',
	alias: 'Umb.TreeItem.DataType',
	name: 'Data Type Tree Item',
	forEntityTypes: ['data-type-root', 'data-type', 'data-type-folder'],
};

export const manifests: Array<ManifestTypes> = [
	treeRepository,
	treeStore,
	tree,
	treeItem,
	...folderManifests,
	...reloadManifests,
];
