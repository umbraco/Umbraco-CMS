import type { ManifestRepository, ManifestStore, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_DATA_TYPE_DETAIL_REPOSITORY_ALIAS, UMB_DATA_TYPE_DETAIL_STORE_ALIAS } from './constants.js';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DATA_TYPE_DETAIL_REPOSITORY_ALIAS,
	name: 'Data Type Detail Repository',
	api: () => import('./data-type-detail.repository.js'),
};

const store: ManifestStore = {
	type: 'store',
	alias: UMB_DATA_TYPE_DETAIL_STORE_ALIAS,
	name: 'Data Type Detail Store',
	api: () => import('./data-type-detail.store.js'),
};

export const manifests: Array<ManifestTypes> = [repository, store];
