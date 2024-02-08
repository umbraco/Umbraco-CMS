import { UmbTemplateItemStore } from './template-item.store.js';
import { UmbTemplateItemRepository } from './template-item.repository.js';
import type { ManifestRepository, ManifestItemStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_TEMPLATE_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.TemplateItem';
export const UMB_TEMPLATE_STORE_ALIAS = 'Umb.Store.TemplateItem';

const itemRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_TEMPLATE_ITEM_REPOSITORY_ALIAS,
	name: 'Template Item Repository',
	api: UmbTemplateItemRepository,
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_TEMPLATE_STORE_ALIAS,
	name: 'Template Item Store',
	api: UmbTemplateItemStore,
};

export const manifests = [itemRepository, itemStore];
