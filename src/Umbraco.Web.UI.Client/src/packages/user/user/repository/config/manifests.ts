import { UMB_USER_CONFIG_REPOSITORY_ALIAS, UMB_USER_CONFIG_STORE_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestStore, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_USER_CONFIG_STORE_ALIAS,
	name: 'User Config Store',
	api: () => import('./user-config.store.js'),
};

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_USER_CONFIG_REPOSITORY_ALIAS,
	name: 'User Config Repository',
	api: () => import('./user-config.repository.js'),
};

export const manifests: Array<ManifestTypes> = [repository, store];
