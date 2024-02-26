import type { ManifestItemStore, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEDIA_TYPE_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.MediaType.Item';
export const UMB_MEDIA_TYPE_ITEM_STORE_ALIAS = 'Umb.Store.MediaType.Item';

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

export const manifests = [itemRepository, itemStore];
