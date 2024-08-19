import { UMB_USER_ITEM_REPOSITORY_ALIAS, UMB_USER_ITEM_STORE_ALIAS } from './constants.js';
import type { ManifestItemStore, ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const itemRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_USER_ITEM_REPOSITORY_ALIAS,
	name: 'User Item Repository',
	api: () => import('./user-item.repository.js'),
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_USER_ITEM_STORE_ALIAS,
	name: 'User Item Store',
	api: () => import('./user-item.store.js'),
};

export const manifests: Array<ManifestTypes> = [itemRepository, itemStore];
