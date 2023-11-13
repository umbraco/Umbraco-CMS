import { UmbDataTypeTreeRepository } from './data-type-tree.repository.js';
import { UmbDataTypeTreeStore } from './data-type.tree.store.js';
import type {
	ManifestRepository,
	ManifestTree,
	ManifestTreeItem,
	ManifestTreeStore,
} from '@umbraco-cms/backoffice/extension-registry';

export const DATA_TYPE_TREE_REPOSITORY_ALIAS = 'Umb.Repository.DataType.Tree';
export const DATA_TYPE_TREE_STORE_ALIAS = 'Umb.Store.DataType.Tree';

const treeRepository: ManifestRepository = {
	type: 'repository',
	alias: DATA_TYPE_TREE_REPOSITORY_ALIAS,
	name: 'Data Type Tree Repository',
	api: UmbDataTypeTreeRepository,
};

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
		repositoryAlias: DATA_TYPE_TREE_REPOSITORY_ALIAS,
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

export const manifests = [treeRepository, treeStore, tree, treeItem];
