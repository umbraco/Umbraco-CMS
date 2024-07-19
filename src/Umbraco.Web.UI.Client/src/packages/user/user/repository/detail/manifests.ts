import { UMB_USER_DETAIL_REPOSITORY_ALIAS, UMB_USER_DETAIL_STORE_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestStore, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_USER_DETAIL_REPOSITORY_ALIAS,
	name: 'User Detail Repository',
	api: () => import('./user-detail.repository.js'),
};

const store: ManifestStore = {
	type: 'store',
	alias: UMB_USER_DETAIL_STORE_ALIAS,
	name: 'User Detail Store',
	api: () => import('./user-detail.store.js'),
};

export const manifests: Array<ManifestTypes> = [repository, store];
