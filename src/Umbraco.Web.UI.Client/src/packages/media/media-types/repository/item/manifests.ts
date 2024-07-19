import { UMB_MEDIA_TYPE_ITEM_REPOSITORY_ALIAS, UMB_MEDIA_TYPE_ITEM_STORE_ALIAS } from './constants.js';
import type { ManifestItemStore, ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const itemRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEDIA_TYPE_ITEM_REPOSITORY_ALIAS,
	name: 'Media Type Item Repository',
	api: () => import('./media-type-item.repository.js'),
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_MEDIA_TYPE_ITEM_STORE_ALIAS,
	name: 'Media Type Item Store',
	api: () => import('./media-type-item.store.js'),
};

export const manifests: Array<ManifestTypes> = [itemRepository, itemStore];
