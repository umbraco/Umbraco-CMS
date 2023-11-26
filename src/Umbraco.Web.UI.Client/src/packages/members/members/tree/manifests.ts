import { UMB_MEMBER_ENTITY_TYPE, UMB_MEMBER_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbMemberTreeRepository } from './member-tree.repository.js';
import { UmbMemberTreeStore } from './member-tree.store.js';
import type {
	ManifestRepository,
	ManifestTree,
	ManifestTreeItem,
	ManifestTreeStore,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEMBER_TREE_REPOSITORY_ALIAS = 'Umb.Repository.Member.Tree';
export const UMB_MEMBER_TREE_STORE_ALIAS = 'Umb.Store.Member.Tree';
export const UMB_MEMBER_TREE_ALIAS = 'Umb.Tree.Member';

const treeRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEMBER_TREE_REPOSITORY_ALIAS,
	name: 'Member Tree Repository',
	api: UmbMemberTreeRepository,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: UMB_MEMBER_TREE_STORE_ALIAS,
	name: 'Member Tree Store',
	api: UmbMemberTreeStore,
};

const tree: ManifestTree = {
	type: 'tree',
	alias: UMB_MEMBER_TREE_ALIAS,
	name: 'Member Tree',
	meta: {
		repositoryAlias: UMB_MEMBER_TREE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'entity',
	alias: 'Umb.TreeItem.Member',
	name: 'Member Tree Item',
	meta: {
		entityTypes: [UMB_MEMBER_ROOT_ENTITY_TYPE, UMB_MEMBER_ENTITY_TYPE],
	},
};

export const manifests = [treeRepository, treeStore, tree, treeItem];
