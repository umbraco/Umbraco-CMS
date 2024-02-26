import { UMB_MEMBER_TYPE_ENTITY_TYPE, UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbMemberTypeTreeRepository } from './member-type-tree.repository.js';
import { UmbMemberTypeTreeStore } from './member-type-tree.store.js';
import { manifests as reloadTreeItemChildrenManifest } from './reload-tree-item-children/manifests.js';
import type {
	ManifestRepository,
	ManifestTree,
	ManifestTreeItem,
	ManifestTreeStore,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEMBER_TYPE_TREE_REPOSITORY_ALIAS = 'Umb.Repository.MemberType.Tree';
export const UMB_MEMBER_TYPE_TREE_STORE_ALIAS = 'Umb.Store.MemberType.Tree';
export const UMB_MEMBER_TYPE_TREE_ALIAS = 'Umb.Tree.MemberType';

const treeRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEMBER_TYPE_TREE_REPOSITORY_ALIAS,
	name: 'Member Type Tree Repository',
	api: () => import('./member-type-tree.repository.js'),
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: UMB_MEMBER_TYPE_TREE_STORE_ALIAS,
	name: 'Member Type Tree Store',
	api: () => import('./member-type-tree.store.js'),
};

const tree: ManifestTree = {
	type: 'tree',
	alias: UMB_MEMBER_TYPE_TREE_ALIAS,
	name: 'Member Type Tree',
	meta: {
		repositoryAlias: UMB_MEMBER_TYPE_TREE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'unique',
	alias: 'Umb.TreeItem.MemberType',
	name: 'Member Type Tree Item',
	meta: {
		entityTypes: [UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE, UMB_MEMBER_TYPE_ENTITY_TYPE],
	},
};

export const manifests = [treeRepository, treeStore, tree, treeItem, ...reloadTreeItemChildrenManifest];
