import { UmbLanguageItemStore } from './language-item.store.js';
import { UmbLanguageItemRepository } from './language-item.repository.js';
import type { ManifestRepository, ManifestItemStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_LANGUAGE_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.LanguageItem';
export const UMB_LANGUAGE_STORE_ALIAS = 'Umb.Store.LanguageItem';

const itemRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_LANGUAGE_ITEM_REPOSITORY_ALIAS,
	name: 'Language Item Repository',
	api: UmbLanguageItemRepository,
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_LANGUAGE_STORE_ALIAS,
	name: 'Language Item Store',
	api: UmbLanguageItemStore,
};

export const manifests = [itemRepository, itemStore];
