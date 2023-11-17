import { UmbMemberDetailRepository } from './member-detail.repository.js';
import { UmbMemberDetailStore } from './member-detail.store.js';
import type { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEMBER_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.Member.Detail';
export const UMB_MEMBER_DETAIL_STORE_ALIAS = 'Umb.Store.Member.Detail';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEMBER_DETAIL_REPOSITORY_ALIAS,
	name: 'Member Detail Detail Repository',
	api: UmbMemberDetailRepository,
};

const store: ManifestStore = {
	type: 'store',
	alias: UMB_MEMBER_DETAIL_STORE_ALIAS,
	name: 'Member Detail Store',
	api: UmbMemberDetailStore,
};

export const manifests = [repository, store];
