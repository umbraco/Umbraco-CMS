import { STYLESHEET_ENTITY_TYPE } from '../config.js';
import { UmbStylesheetTreeRepository } from './stylesheet-tree.repository.js';
import { UmbStylesheetTreeStore } from './stylesheet.tree.store.js';
import type {
	ManifestRepository,
	ManifestTree,
	ManifestTreeItem,
	ManifestTreeStore,
} from '@umbraco-cms/backoffice/extension-registry';

export const STYLESHEET_TREE_ALIAS = 'Umb.Tree.Stylesheet';
export const STYLESHEET_TREE_REPOSITORY_ALIAS = 'Umb.Repository.StylesheetTree';
export const STYLESHEET_TREE_STORE_ALIAS = 'Umb.Store.StylesheetTree';

const treeRepository: ManifestRepository = {
	type: 'repository',
	alias: STYLESHEET_TREE_REPOSITORY_ALIAS,
	name: 'Stylesheet Tree Repository',
	api: UmbStylesheetTreeRepository,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: STYLESHEET_TREE_STORE_ALIAS,
	name: 'Stylesheet Tree Store',
	api: UmbStylesheetTreeStore,
};

const tree: ManifestTree = {
	type: 'tree',
	alias: STYLESHEET_TREE_ALIAS,
	name: 'Stylesheet Tree',
	weight: 10,
	meta: {
		repositoryAlias: STYLESHEET_TREE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'fileSystem',
	alias: 'Umb.TreeItem.Stylesheet',
	name: 'Stylesheet Tree Item',
	meta: {
		entityTypes: ['stylesheet-root', STYLESHEET_ENTITY_TYPE],
	},
};

export const manifests = [treeRepository, treeStore, tree, treeItem];
