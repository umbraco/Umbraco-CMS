import { UmbDictionaryItemStore } from './dictionary-item.store.js';
import { UmbDictionaryItemRepository } from './dictionary-item.repository.js';
import { UMB_DICTIONARY_ITEM_REPOSITORY_ALIAS, UMB_DICTIONARY_STORE_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestItemStore, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

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

export const manifests: Array<ManifestTypes> = [itemRepository, itemStore];
