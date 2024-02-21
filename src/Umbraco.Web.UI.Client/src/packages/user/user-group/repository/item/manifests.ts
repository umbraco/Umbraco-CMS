import { UmbUserGroupItemStore } from './user-group-item.store.js';
import { UmbUserGroupItemRepository } from './user-group-item.repository.js';
import type { ManifestRepository, ManifestItemStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_USER_GROUP_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.UserGroupItem';
export const UMB_USER_GROUP_STORE_ALIAS = 'Umb.Store.UserGroupItem';

const itemRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_USER_GROUP_ITEM_REPOSITORY_ALIAS,
	name: 'User Group Item Repository',
	api: UmbUserGroupItemRepository,
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_USER_GROUP_STORE_ALIAS,
	name: 'User Group Item Store',
	api: UmbUserGroupItemStore,
};

export const manifests = [itemRepository, itemStore];
