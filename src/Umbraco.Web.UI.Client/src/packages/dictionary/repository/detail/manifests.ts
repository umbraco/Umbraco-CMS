import { UMB_DICTIONARY_DETAIL_REPOSITORY_ALIAS, UMB_DICTIONARY_DETAIL_STORE_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestStore, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DICTIONARY_DETAIL_REPOSITORY_ALIAS,
	name: 'Dictionary Detail Repository',
	api: () => import('./dictionary-detail.repository.js'),
};

const store: ManifestStore = {
	type: 'store',
	alias: UMB_DICTIONARY_DETAIL_STORE_ALIAS,
	name: 'Dictionary Detail Store',
	api: () => import('./dictionary-detail.store.js'),
};

export const manifests: Array<ManifestTypes> = [repository, store];
