import { UmbMediaTypeItemRepository } from './media-type-item.repository.js';
import { UmbMediaTypeItemStore } from './media-type-item.store.js';
import { ManifestItemStore, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEDIA_TYPE_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.MediaType.Item';
export const UMB_MEDIA_TYPE_ITEM_STORE_ALIAS = 'Umb.Store.MediaType.Item';

const itemRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEDIA_TYPE_ITEM_REPOSITORY_ALIAS,
	name: 'Media Type Item Repository',
	api: UmbMediaTypeItemRepository,
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_MEDIA_TYPE_ITEM_STORE_ALIAS,
	name: 'Media Type Item Store',
	api: UmbMediaTypeItemStore,
};

export const manifests = [itemRepository, itemStore];
