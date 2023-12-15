import { UmbStaticFileTreeRepository } from './static-file-tree.repository.js';
import { UmbStaticFileTreeStore } from './static-file-tree.store.js';
import type { ManifestRepository, ManifestTree, ManifestTreeStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_STATIC_FILE_TREE_REPOSITORY_ALIAS = 'Umb.Repository.StaticFile.Tree';
export const UMB_STATIC_FILE_TREE_STORE_ALIAS = 'Umb.Store.StaticFile.Tree';
export const UMB_STATIC_FILE_TREE_ALIAS = 'Umb.Tree.StaticFile';

const treeRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_STATIC_FILE_TREE_REPOSITORY_ALIAS,
	name: 'Static File Tree Repository',
	api: UmbStaticFileTreeRepository,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: UMB_STATIC_FILE_TREE_STORE_ALIAS,
	name: 'Static File Tree Store',
	api: UmbStaticFileTreeStore,
};

const tree: ManifestTree = {
	type: 'tree',
	alias: UMB_STATIC_FILE_TREE_ALIAS,
	name: 'Static File Tree',
	meta: {
		repositoryAlias: UMB_STATIC_FILE_TREE_REPOSITORY_ALIAS,
	},
};

export const manifests = [treeRepository, treeStore, tree];
