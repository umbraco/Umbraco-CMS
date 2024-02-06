import { UMB_MEMBER_GROUP_ENTITY_TYPE, UMB_MEMBER_GROUP_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbMemberGroupTreeRepository } from './member-group-tree.repository.js';
import { UmbMemberGroupTreeStore } from './member-group-tree.store.js';
import { manifests as reloadTreeItemChildrenManifest } from './reload-tree-item-children/manifests.js';
import type {
	ManifestRepository,
	ManifestTree,
	ManifestTreeItem,
	ManifestTreeStore,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEMBER_GROUP_TREE_REPOSITORY_ALIAS = 'Umb.Repository.MemberGroup.Tree';
export const UMB_MEMBER_GROUP_TREE_STORE_ALIAS = 'Umb.Store.MemberGroup.Tree';
export const UMB_MEMBER_GROUP_TREE_ALIAS = 'Umb.Tree.MemberGroup';

const treeRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEMBER_GROUP_TREE_REPOSITORY_ALIAS,
	name: 'MemberGroup Tree Repository',
	api: UmbMemberGroupTreeRepository,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: UMB_MEMBER_GROUP_TREE_STORE_ALIAS,
	name: 'MemberGroup Tree Store',
	api: UmbMemberGroupTreeStore,
};

const tree: ManifestTree = {
	type: 'tree',
	alias: UMB_MEMBER_GROUP_TREE_ALIAS,
	name: 'MemberGroup Tree',
	meta: {
		repositoryAlias: UMB_MEMBER_GROUP_TREE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'entity',
	alias: 'Umb.TreeItem.MemberGroup',
	name: 'MemberGroup Tree Item',
	meta: {
		entityTypes: [UMB_MEMBER_GROUP_ROOT_ENTITY_TYPE, UMB_MEMBER_GROUP_ENTITY_TYPE],
	},
};

export const manifests = [treeRepository, treeStore, tree, treeItem, ...reloadTreeItemChildrenManifest];
