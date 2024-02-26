import type { ManifestRepository, ManifestItemStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_LANGUAGE_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.LanguageItem';
export const UMB_LANGUAGE_STORE_ALIAS = 'Umb.Store.LanguageItem';

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

export const manifests = [itemRepository, itemStore];
