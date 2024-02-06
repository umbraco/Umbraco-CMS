import { UmbUserDetailRepository } from './user-detail.repository.js';
import { UmbUserDetailStore } from './user-detail.store.js';
import type { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_USER_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.User.Detail';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_USER_DETAIL_REPOSITORY_ALIAS,
	name: 'User Detail Repository',
	api: UmbUserDetailRepository,
};

export const UMB_USER_DETAIL_STORE_ALIAS = 'Umb.Store.User.Detail';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_USER_DETAIL_STORE_ALIAS,
	name: 'User Detail Store',
	api: UmbUserDetailStore,
};

export const manifests = [repository, store];
