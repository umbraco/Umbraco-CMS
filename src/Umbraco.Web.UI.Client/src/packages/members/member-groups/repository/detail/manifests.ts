import { UmbMemberGroupDetailRepository } from './member-group-detail.repository.js';
import { UmbMemberGroupDetailStore } from './member-group-detail.store.js';
import type { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEMBER_GROUP_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.MemberGroup.Detail';
export const UMB_MEMBER_GROUP_DETAIL_STORE_ALIAS = 'Umb.Store.MemberGroup.Detail';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEMBER_GROUP_DETAIL_REPOSITORY_ALIAS,
	name: 'MemberGroup Detail Detail Repository',
	api: UmbMemberGroupDetailRepository,
};

const store: ManifestStore = {
	type: 'store',
	alias: UMB_MEMBER_GROUP_DETAIL_STORE_ALIAS,
	name: 'MemberGroup Detail Store',
	api: UmbMemberGroupDetailStore,
};

export const manifests = [repository, store];
