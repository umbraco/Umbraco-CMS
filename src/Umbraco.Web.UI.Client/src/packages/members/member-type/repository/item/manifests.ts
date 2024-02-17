import { UmbMemberTypeItemStore } from './member-type-item.store.js';
import { UmbMemberTypeItemRepository } from './member-type-item.repository.js';
import type { ManifestRepository, ManifestItemStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEMBER_TYPE_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.MemberTypeItem';
export const UMB_MEMBER_TYPE_STORE_ALIAS = 'Umb.Store.MemberTypeItem';

const itemRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEMBER_TYPE_ITEM_REPOSITORY_ALIAS,
	name: 'Member Type Item Repository',
	api: UmbMemberTypeItemRepository,
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_MEMBER_TYPE_STORE_ALIAS,
	name: 'Member Type Item Store',
	api: UmbMemberTypeItemStore,
};

export const manifests = [itemRepository, itemStore];
