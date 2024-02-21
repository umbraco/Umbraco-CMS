import { UmbMemberTypeDetailRepository } from './member-type-detail.repository.js';
import { UmbMemberTypeDetailStore } from './member-type-detail.store.js';
import type { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEMBER_TYPE_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.MemberType.Detail';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEMBER_TYPE_DETAIL_REPOSITORY_ALIAS,
	name: 'Member Type Detail Repository',
	api: UmbMemberTypeDetailRepository,
};

export const UMB_MEMBER_TYPE_DETAIL_STORE_ALIAS = 'Umb.Store.MemberType.Detail';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_MEMBER_TYPE_DETAIL_STORE_ALIAS,
	name: 'Member Type Detail Store',
	api: UmbMemberTypeDetailStore,
};

export const manifests = [repository, store];
