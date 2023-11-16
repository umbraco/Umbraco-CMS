import { UmbMemberTypeRepository } from './member-type.repository.js';
import { UmbMemberTypeStore } from './member-type.store.js';
import type { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEMBER_TYPE_REPOSITORY_ALIAS = 'Umb.Repository.MemberType';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEMBER_TYPE_REPOSITORY_ALIAS,
	name: 'Member Type Repository',
	api: UmbMemberTypeRepository,
};

export const UMB_MEMBER_TYPE_STORE_ALIAS = 'Umb.Store.MemberType';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_MEMBER_TYPE_STORE_ALIAS,
	name: 'Member Type Store',
	api: UmbMemberTypeStore,
};

export const manifests = [store, repository];
