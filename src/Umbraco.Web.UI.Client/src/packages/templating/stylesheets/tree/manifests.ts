import { UMB_STYLESHEET_ENTITY_TYPE } from '../entity-type.js';
import { UmbStylesheetTreeRepository } from './stylesheet-tree.repository.js';
import { UmbStylesheetTreeStore } from './stylesheet-tree.store.js';
import type {
	ManifestRepository,
	ManifestTree,
	ManifestTreeItem,
	ManifestTreeStore,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_STYLESHEET_TREE_ALIAS = 'Umb.Tree.Stylesheet';
export const UMB_STYLESHEET_TREE_REPOSITORY_ALIAS = 'Umb.Repository.StylesheetTree';
export const UMB_STYLESHEET_TREE_STORE_ALIAS = 'Umb.Store.StylesheetTree';

const treeRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_STYLESHEET_TREE_REPOSITORY_ALIAS,
	name: 'Stylesheet Tree Repository',
	api: UmbStylesheetTreeRepository,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: UMB_STYLESHEET_TREE_STORE_ALIAS,
	name: 'Stylesheet Tree Store',
	api: UmbStylesheetTreeStore,
};

const tree: ManifestTree = {
	type: 'tree',
	alias: UMB_STYLESHEET_TREE_ALIAS,
	name: 'Stylesheet Tree',
	weight: 10,
	meta: {
		repositoryAlias: UMB_STYLESHEET_TREE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'fileSystem',
	alias: 'Umb.TreeItem.Stylesheet',
	name: 'Stylesheet Tree Item',
	meta: {
		entityTypes: ['stylesheet-root', UMB_STYLESHEET_ENTITY_TYPE],
	},
};

export const manifests = [treeRepository, treeStore, tree, treeItem];
