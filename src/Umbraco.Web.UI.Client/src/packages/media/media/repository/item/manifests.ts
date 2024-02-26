import type { ManifestRepository, ManifestItemStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEDIA_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.MediaItem';
export const UMB_MEDIA_STORE_ALIAS = 'Umb.Store.MediaItem';

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

export const manifests = [itemRepository, itemStore];
