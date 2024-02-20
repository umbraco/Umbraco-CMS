import { UmbMemberGroupItemStore } from './member-group-item.store.js';
import { UmbMemberGroupItemRepository } from './member-group-item.repository.js';
import type { ManifestRepository, ManifestItemStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEMBER_GROUP_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.MemberGroupItem';
export const UMB_MEMBER_GROUP_STORE_ALIAS = 'Umb.Store.MemberGroupItem';

const itemRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEMBER_GROUP_ITEM_REPOSITORY_ALIAS,
	name: 'Member Group Item Repository',
	api: UmbMemberGroupItemRepository,
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_MEMBER_GROUP_STORE_ALIAS,
	name: 'Member Group Item Store',
	api: UmbMemberGroupItemStore,
};

export const manifests = [itemRepository, itemStore];
