import { UMB_SCRIPT_ENTITY_TYPE, UMB_SCRIPT_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbScriptTreeRepository } from './script-tree.repository.js';
import { UmbScriptTreeStore } from './script-tree.store.js';
import type {
	ManifestRepository,
	ManifestTree,
	ManifestTreeItem,
	ManifestTreeStore,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_SCRIPT_TREE_REPOSITORY_ALIAS = 'Umb.Repository.Script.Tree';
export const UMB_SCRIPT_TREE_STORE_ALIAS = 'Umb.Store.Script.Tree';
export const UMB_SCRIPT_TREE_ALIAS = 'Umb.Tree.Script';

const treeRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_SCRIPT_TREE_REPOSITORY_ALIAS,
	name: 'Script Tree Repository',
	api: UmbScriptTreeRepository,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: UMB_SCRIPT_TREE_STORE_ALIAS,
	name: 'Script Tree Store',
	api: UmbScriptTreeStore,
};

const tree: ManifestTree = {
	type: 'tree',
	alias: UMB_SCRIPT_TREE_ALIAS,
	name: 'Script Tree',
	meta: {
		repositoryAlias: UMB_SCRIPT_TREE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'fileSystem',
	alias: 'Umb.TreeItem.Script',
	name: 'Script Tree Item',
	meta: {
		entityTypes: [UMB_SCRIPT_ROOT_ENTITY_TYPE, UMB_SCRIPT_ENTITY_TYPE],
	},
};

export const manifests = [treeRepository, treeStore, tree, treeItem];
