import { UmbDataTypeItemStore } from './data-type-item.store.js';
import { UmbDataTypeItemRepository } from './data-type-item.repository.js';
import type { ManifestRepository, ManifestItemStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.DataTypeItem';
export const UMB_DATA_TYPE_STORE_ALIAS = 'Umb.Store.DataTypeItem';

const itemRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS,
	name: 'Data Type Item Repository',
	api: UmbDataTypeItemRepository,
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_DATA_TYPE_STORE_ALIAS,
	name: 'Data Type Item Store',
	api: UmbDataTypeItemStore,
};

export const manifests = [itemRepository, itemStore];
