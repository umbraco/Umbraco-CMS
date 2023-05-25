import { UmbMemberRepository } from './member.repository.js';
import { UmbMemberStore } from './member.store.js';
import { UmbMemberTreeStore } from './member.tree.store.js';
import type { ManifestStore, ManifestTreeStore, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const MEMBER_REPOSITORY_ALIAS = 'Umb.Repository.Member';

const repository: ManifestRepository = {
	type: 'repository',
	alias: MEMBER_REPOSITORY_ALIAS,
	name: 'Member Repository',
	class: UmbMemberRepository,
};

export const MEMBER_STORE_ALIAS = 'Umb.Store.Member';
export const MEMBER_TREE_STORE_ALIAS = 'Umb.Store.MemberTree';

const store: ManifestStore = {
	type: 'store',
	alias: MEMBER_STORE_ALIAS,
	name: 'Member Store',
	class: UmbMemberStore,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: MEMBER_TREE_STORE_ALIAS,
	name: 'Member Tree Store',
	class: UmbMemberTreeStore,
};

export const manifests = [store, treeStore, repository];
