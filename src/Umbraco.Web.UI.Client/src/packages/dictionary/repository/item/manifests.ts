import { UMB_DICTIONARY_ITEM_REPOSITORY_ALIAS, UMB_DICTIONARY_STORE_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestItemStore, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const itemRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DICTIONARY_ITEM_REPOSITORY_ALIAS,
	name: 'Dictionary Item Repository',
	api: () => import('./dictionary-item.repository.js'),
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_DICTIONARY_STORE_ALIAS,
	name: 'Dictionary Item Store',
	api: () => import('./dictionary-item.store.js'),
};

export const manifests: Array<ManifestTypes> = [itemRepository, itemStore];
