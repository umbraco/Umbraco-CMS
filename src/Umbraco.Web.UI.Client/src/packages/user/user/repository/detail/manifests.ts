import type { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_USER_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.User.Detail';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_USER_DETAIL_REPOSITORY_ALIAS,
	name: 'User Detail Repository',
	api: () => import('./user-detail.repository.js'),
};

export const UMB_USER_DETAIL_STORE_ALIAS = 'Umb.Store.User.Detail';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_USER_DETAIL_STORE_ALIAS,
	name: 'User Detail Store',
	api: () => import('./user-detail.store.js'),
};

export const manifests = [repository, store];
