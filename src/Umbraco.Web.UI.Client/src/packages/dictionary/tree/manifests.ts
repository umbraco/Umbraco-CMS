import { UMB_DICTIONARY_ENTITY_TYPE, UMB_DICTIONARY_ROOT_ENTITY_TYPE } from '../entity.js';
import {
	UMB_DICTIONARY_TREE_ALIAS,
	UMB_DICTIONARY_TREE_REPOSITORY_ALIAS,
	UMB_DICTIONARY_TREE_STORE_ALIAS,
} from './constants.js';
import { UmbDictionaryTreeStore } from './dictionary-tree.store.js';
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
	alias: UMB_DICTIONARY_TREE_REPOSITORY_ALIAS,
	name: 'Dictionary Tree Repository',
	api: () => import('./dictionary-tree.repository.js'),
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: UMB_DICTIONARY_TREE_STORE_ALIAS,
	name: 'Dictionary Tree Store',
	api: () => import('./dictionary-tree.store.js'),
};

const tree: ManifestTree = {
	type: 'tree',
	kind: 'default',
	alias: UMB_DICTIONARY_TREE_ALIAS,
	name: 'Dictionary Tree',
	meta: {
		repositoryAlias: UMB_DICTIONARY_TREE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'default',
	alias: 'Umb.TreeItem.Dictionary',
	name: 'Dictionary Tree Item',
	forEntityTypes: [UMB_DICTIONARY_ROOT_ENTITY_TYPE, UMB_DICTIONARY_ENTITY_TYPE],
};

export const manifests: Array<ManifestTypes> = [
	treeRepository,
	treeStore,
	tree,
	treeItem,
	...reloadTreeItemChildrenManifests,
];
