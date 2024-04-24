import type { ManifestRepository, ManifestItemStore, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.DataType.Item';
export const UMB_DATA_TYPE_STORE_ALIAS = 'Umb.Store.DataType.Item';

const itemRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS,
	name: 'Data Type Item Repository',
	api: () => import('./data-type-item.repository.js'),
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_DATA_TYPE_STORE_ALIAS,
	name: 'Data Type Item Store',
	api: () => import('./data-type-item.store.js'),
};

export const manifests: Array<ManifestTypes> = [itemRepository, itemStore];
