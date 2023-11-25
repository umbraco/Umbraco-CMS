import { UmbLanguageRepository } from '../repository/language.repository.js';
import { UmbLanguageStore } from './language.store.js';
import { UmbLanguageItemStore } from './language-item.store.js';
import type { ManifestStore, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_LANGUAGE_REPOSITORY_ALIAS = 'Umb.Repository.Language';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_LANGUAGE_REPOSITORY_ALIAS,
	name: 'Languages Repository',
	api: UmbLanguageRepository,
};

export const UMB_LANGUAGE_STORE_ALIAS = 'Umb.Store.Language';
export const UMB_LANGUAGE_ITEM_STORE_ALIAS = 'Umb.Store.LanguageItem';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_LANGUAGE_STORE_ALIAS,
	name: 'Language Store',
	api: UmbLanguageStore,
};

const itemStore = {
	type: 'itemStore',
	alias: UMB_LANGUAGE_ITEM_STORE_ALIAS,
	name: 'Language Item Store',
	api: UmbLanguageItemStore,
};

export const manifests = [repository, store, itemStore];
