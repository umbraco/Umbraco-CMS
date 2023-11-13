import { DATA_TYPE_REPOSITORY_ALIAS } from '../repository/manifests.js';
import { UmbDataTypeTreeStore } from './data-type.tree.store.js';
import type { ManifestTree, ManifestTreeItem, ManifestTreeStore } from '@umbraco-cms/backoffice/extension-registry';

export const DATA_TYPE_TREE_STORE_ALIAS = 'Umb.Store.DataType.Tree';

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: DATA_TYPE_TREE_STORE_ALIAS,
	name: 'Data Type Tree Store',
	api: UmbDataTypeTreeStore,
};

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.DataTypes',
	name: 'Data Types Tree',
	meta: {
		repositoryAlias: DATA_TYPE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'entity',
	alias: 'Umb.TreeItem.DataType',
	name: 'Data Type Tree Item',
	meta: {
		entityTypes: ['data-type-root', 'data-type'],
	},
};

export const manifests = [treeStore, tree, treeItem];
