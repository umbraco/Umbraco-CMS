import { UmbStaticFileItemStore } from './static-file-item.store.js';
import { UmbStaticFileItemRepository } from './static-file-item.repository.js';
import type { ManifestRepository, ManifestItemStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_STATIC_FILE_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.StaticFileItem';
export const UMB_STATIC_FILE_STORE_ALIAS = 'Umb.Store.StaticFileItem';

const itemRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_STATIC_FILE_ITEM_REPOSITORY_ALIAS,
	name: 'Static File Item Repository',
	api: UmbStaticFileItemRepository,
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_STATIC_FILE_STORE_ALIAS,
	name: 'Static File Item Store',
	api: UmbStaticFileItemStore,
};

export const manifests = [itemRepository, itemStore];
