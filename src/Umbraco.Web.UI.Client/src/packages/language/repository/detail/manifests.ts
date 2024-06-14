import { UMB_LANGUAGE_DETAIL_REPOSITORY_ALIAS, UMB_LANGUAGE_DETAIL_STORE_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestStore, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_LANGUAGE_DETAIL_REPOSITORY_ALIAS,
	name: 'Language Detail Repository',
	api: () => import('./language-detail.repository.js'),
};

const store: ManifestStore = {
	type: 'store',
	alias: UMB_LANGUAGE_DETAIL_STORE_ALIAS,
	name: 'Language Detail Store',
	api: () => import('./language-detail.store.js'),
};

export const manifests: Array<ManifestTypes> = [repository, store];
