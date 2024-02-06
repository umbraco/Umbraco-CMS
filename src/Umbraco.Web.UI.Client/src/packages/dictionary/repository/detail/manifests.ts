import { UmbDictionaryDetailRepository } from './dictionary-detail.repository.js';
import { UmbDictionaryDetailStore } from './dictionary-detail.store.js';
import type { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DICTIONARY_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.Dictionary.Detail';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DICTIONARY_DETAIL_REPOSITORY_ALIAS,
	name: 'Dictionary Detail Repository',
	api: UmbDictionaryDetailRepository,
};

export const UMB_DICTIONARY_DETAIL_STORE_ALIAS = 'Umb.Store.Dictionary.Detail';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_DICTIONARY_DETAIL_STORE_ALIAS,
	name: 'Dictionary Detail Store',
	api: UmbDictionaryDetailStore,
};

export const manifests = [repository, store];
