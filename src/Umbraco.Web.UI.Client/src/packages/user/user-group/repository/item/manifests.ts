import type { ManifestRepository, ManifestItemStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_USER_GROUP_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.UserGroupItem';
export const UMB_USER_GROUP_STORE_ALIAS = 'Umb.Store.UserGroupItem';

const itemRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_USER_GROUP_ITEM_REPOSITORY_ALIAS,
	name: 'User Group Item Repository',
	api: () => import('./user-group-item.repository.js'),
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_USER_GROUP_STORE_ALIAS,
	name: 'User Group Item Store',
	api: () => import('./user-group-item.store.js'),
};

export const manifests = [itemRepository, itemStore];
