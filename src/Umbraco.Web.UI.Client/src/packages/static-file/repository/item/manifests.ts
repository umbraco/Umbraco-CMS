import type { ManifestRepository, ManifestItemStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_STATIC_FILE_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.StaticFileItem';
export const UMB_STATIC_FILE_STORE_ALIAS = 'Umb.Store.StaticFileItem';

const itemRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_STATIC_FILE_ITEM_REPOSITORY_ALIAS,
	name: 'Static File Item Repository',
	api: () => import('./static-file-item.repository.js'),
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_STATIC_FILE_STORE_ALIAS,
	name: 'Static File Item Store',
	api: () => import('./static-file-item.store.js'),
};

export const manifests = [itemRepository, itemStore];
