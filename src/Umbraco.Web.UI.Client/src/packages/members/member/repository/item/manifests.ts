import type { ManifestRepository, ManifestItemStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEMBER_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.MemberItem';
export const UMB_MEMBER_STORE_ALIAS = 'Umb.Store.MemberItem';

const itemRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEMBER_ITEM_REPOSITORY_ALIAS,
	name: 'Member Item Repository',
	api: () => import('./member-item.repository.js'),
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_MEMBER_STORE_ALIAS,
	name: 'Member Item Store',
	api: () => import('./member-item.store.js'),
};

export const manifests = [itemRepository, itemStore];
