import { UMB_DICTIONARY_DETAIL_REPOSITORY_ALIAS, UMB_DICTIONARY_DETAIL_STORE_ALIAS } from './constants.js';
import { UmbDictionaryDetailRepository } from './dictionary-detail.repository.js';
import { UmbDictionaryDetailStore } from './dictionary-detail.store.js';
import type { ManifestRepository, ManifestStore, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DICTIONARY_DETAIL_REPOSITORY_ALIAS,
	name: 'Dictionary Detail Repository',
	api: UmbDictionaryDetailRepository,
};

const store: ManifestStore = {
	type: 'store',
	alias: UMB_DICTIONARY_DETAIL_STORE_ALIAS,
	name: 'Dictionary Detail Store',
	api: UmbDictionaryDetailStore,
};

export const manifests: Array<ManifestTypes> = [repository, store];
