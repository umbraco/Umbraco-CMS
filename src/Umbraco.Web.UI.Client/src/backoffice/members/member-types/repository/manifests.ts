import { UmbMemberTypeRepository } from './member-type.repository';
import { UmbMemberTypeStore } from './member-type.store';
import { UmbMemberTypeTreeStore } from './member-type.tree.store';
import type { ManifestRepository, ManifestStore, ManifestTreeStore } from '@umbraco-cms/extensions-registry';

export const MEMBER_TYPES_REPOSITORY_ALIAS = 'Umb.Repository.MemberType';

const repository: ManifestRepository = {
	type: 'repository',
	alias: MEMBER_TYPES_REPOSITORY_ALIAS,
	name: 'Member Types Repository',
	class: UmbMemberTypeRepository,
};

export const MEMBER_TYPE_STORE_ALIAS = 'Umb.Store.MemberType';
export const MEMBER_TYPE_TREE_STORE_ALIAS = 'Umb.Store.MemberTypeTree';

const store: ManifestStore = {
	type: 'store',
	alias: MEMBER_TYPE_STORE_ALIAS,
	name: 'Member Type Store',
	class: UmbMemberTypeStore,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: MEMBER_TYPE_TREE_STORE_ALIAS,
	name: 'Member Type Tree Store',
	class: UmbMemberTypeTreeStore,
};

export const manifests = [store, treeStore, repository];
