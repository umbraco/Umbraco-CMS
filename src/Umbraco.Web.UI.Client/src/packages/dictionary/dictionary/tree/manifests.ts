import { UMB_DICTIONARY_ENTITY_TYPE, UMB_DICTIONARY_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbDictionaryTreeRepository } from './dictionary-tree.repository.js';
import { UmbDictionaryTreeStore } from './dictionary-tree.store.js';
import { manifests as reloadTreeItemChildrenManifests } from './reload-tree-item-children/manifests.js';
import type {
	ManifestRepository,
	ManifestTree,
	ManifestTreeItem,
	ManifestTreeStore,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DICTIONARY_TREE_REPOSITORY_ALIAS = 'Umb.Repository.Dictionary.Tree';
export const UMB_DICTIONARY_TREE_STORE_ALIAS = 'Umb.Store.Dictionary.Tree';
export const UMB_DICTIONARY_TREE_ALIAS = 'Umb.Tree.Dictionary';

const treeRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DICTIONARY_TREE_REPOSITORY_ALIAS,
	name: 'Dictionary Tree Repository',
	api: UmbDictionaryTreeRepository,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: UMB_DICTIONARY_TREE_STORE_ALIAS,
	name: 'Dictionary Tree Store',
	api: UmbDictionaryTreeStore,
};

const tree: ManifestTree = {
	type: 'tree',
	alias: UMB_DICTIONARY_TREE_ALIAS,
	name: 'Dictionary Tree',
	meta: {
		repositoryAlias: UMB_DICTIONARY_TREE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'entity',
	alias: 'Umb.TreeItem.Dictionary',
	name: 'Dictionary Tree Item',
	meta: {
		entityTypes: [UMB_DICTIONARY_ROOT_ENTITY_TYPE, UMB_DICTIONARY_ENTITY_TYPE],
	},
};

export const manifests = [treeRepository, treeStore, tree, treeItem, ...reloadTreeItemChildrenManifests];
