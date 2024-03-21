import { UmbRelationTypeItemStore } from './relation-type-item.store.js';
import { UmbRelationTypeItemRepository } from './relation-type-item.repository.js';
import type { ManifestRepository, ManifestItemStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_RELATION_TYPE_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.RelationType.Item';
export const UMB_RELATION_TYPE_STORE_ALIAS = 'Umb.Store.RelationType.Item';

const itemRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_RELATION_TYPE_ITEM_REPOSITORY_ALIAS,
	name: 'Relation Type Item Repository',
	api: UmbRelationTypeItemRepository,
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_RELATION_TYPE_STORE_ALIAS,
	name: 'Relation Type Item Store',
	api: UmbRelationTypeItemStore,
};

export const manifests = [itemRepository, itemStore];
