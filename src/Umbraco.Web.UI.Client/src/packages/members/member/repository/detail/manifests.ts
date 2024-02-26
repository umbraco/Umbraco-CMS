import type { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEMBER_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.Member.Detail';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEMBER_DETAIL_REPOSITORY_ALIAS,
	name: 'Member Detail Repository',
	api: () => import('./member-detail.repository.js'),
};

export const UMB_MEMBER_DETAIL_STORE_ALIAS = 'Umb.Store.Member.Detail';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_MEMBER_DETAIL_STORE_ALIAS,
	name: 'Member Detail Store',
	api: () => import('./member-detail.store.js'),
};

export const manifests = [repository, store];
