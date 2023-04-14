import { UmbMemberGroupRepository } from './member-group.repository';
import { UmbMemberGroupStore } from './member-group.store';
import { UmbMemberGroupTreeStore } from './member-group.tree.store';
import type { ManifestStore, ManifestTreeStore, ManifestRepository } from '@umbraco-cms/backoffice/extensions-registry';

export const MEMBER_GROUP_REPOSITORY_ALIAS = 'Umb.Repository.MemberGroup';

const repository: ManifestRepository = {
	type: 'repository',
	alias: MEMBER_GROUP_REPOSITORY_ALIAS,
	name: 'Member Group Repository',
	class: UmbMemberGroupRepository,
};

export const MEMBER_GROUP_STORE_ALIAS = 'Umb.Store.MemberGroup';
export const MEMBER_GROUP_TREE_STORE_ALIAS = 'Umb.Store.MemberGroupTree';

const store: ManifestStore = {
	type: 'store',
	alias: MEMBER_GROUP_STORE_ALIAS,
	name: 'Member Group Store',
	class: UmbMemberGroupStore,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: MEMBER_GROUP_TREE_STORE_ALIAS,
	name: 'Member Group Tree Store',
	class: UmbMemberGroupTreeStore,
};

export const manifests = [store, treeStore, repository];
