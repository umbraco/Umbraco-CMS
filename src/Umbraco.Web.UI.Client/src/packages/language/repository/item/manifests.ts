import { UMB_LANGUAGE_ITEM_REPOSITORY_ALIAS, UMB_LANGUAGE_STORE_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestItemStore, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const itemRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_LANGUAGE_ITEM_REPOSITORY_ALIAS,
	name: 'Language Item Repository',
	api: () => import('./language-item.repository.js'),
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_LANGUAGE_STORE_ALIAS,
	name: 'Language Item Store',
	api: () => import('./language-item.store.js'),
};

export const manifests: Array<ManifestTypes> = [itemRepository, itemStore];
