import { UMB_MEDIA_ITEM_REPOSITORY_ALIAS, UMB_MEDIA_STORE_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestItemStore, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const itemRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEDIA_ITEM_REPOSITORY_ALIAS,
	name: 'Media Item Repository',
	api: () => import('./media-item.repository.js'),
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_MEDIA_STORE_ALIAS,
	name: 'Media Item Store',
	api: () => import('./media-item.store.js'),
};

export const manifests: Array<ManifestTypes> = [itemRepository, itemStore];
