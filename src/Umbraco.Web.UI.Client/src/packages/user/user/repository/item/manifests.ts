import { UmbUserItemStore } from './user-item.store.js';
import { UmbUserItemRepository } from './user-item.repository.js';
import { ManifestItemStore, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_USER_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.User.Item';
export const UMB_USER_ITEM_STORE_ALIAS = 'Umb.ItemStore.User';

const itemRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_USER_ITEM_REPOSITORY_ALIAS,
	name: 'User Item Repository',
	api: UmbUserItemRepository,
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_USER_ITEM_STORE_ALIAS,
	name: 'User Item Store',
	api: UmbUserItemStore,
};

export const manifests = [itemRepository, itemStore];
