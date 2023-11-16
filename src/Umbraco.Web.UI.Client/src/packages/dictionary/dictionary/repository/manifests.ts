import { UmbDictionaryRepository } from './dictionary.repository.js';
import { UmbDictionaryStore } from './dictionary.store.js';
import { ManifestStore, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DICTIONARY_REPOSITORY_ALIAS = 'Umb.Repository.Dictionary';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DICTIONARY_REPOSITORY_ALIAS,
	name: 'Dictionary Repository',
	api: UmbDictionaryRepository,
};

export const UMB_DICTIONARY_STORE_ALIAS = 'Umb.Store.Dictionary';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_DICTIONARY_STORE_ALIAS,
	name: 'Dictionary Store',
	api: UmbDictionaryStore,
};

export const manifests = [repository, store];
