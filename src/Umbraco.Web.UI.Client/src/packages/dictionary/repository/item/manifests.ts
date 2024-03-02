import { UmbDictionaryItemStore } from './dictionary-item.store.js';
import { UmbDictionaryItemRepository } from './dictionary-item.repository.js';
import type { ManifestRepository, ManifestItemStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DICTIONARY_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.Dictionary.Item';
export const UMB_DICTIONARY_STORE_ALIAS = 'Umb.Store.Dictionary.Item';

const itemRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DICTIONARY_ITEM_REPOSITORY_ALIAS,
	name: 'Dictionary Item Repository',
	api: UmbDictionaryItemRepository,
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_DICTIONARY_STORE_ALIAS,
	name: 'Dictionary Item Store',
	api: UmbDictionaryItemStore,
};

export const manifests = [itemRepository, itemStore];
