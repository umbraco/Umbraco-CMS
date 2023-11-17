import { UmbTemplateRepository } from '../repository/template.repository.js';
import { UmbTemplateStore } from './template.store.js';
import { UmbTemplateItemStore } from './template-item.store.js';
import { ManifestStore, ManifestRepository, ManifestItemStore } from '@umbraco-cms/backoffice/extension-registry';

export const TEMPLATE_REPOSITORY_ALIAS = 'Umb.Repository.Template';

const repository: ManifestRepository = {
	type: 'repository',
	alias: TEMPLATE_REPOSITORY_ALIAS,
	name: 'Template Repository',
	api: UmbTemplateRepository,
};

export const UMB_TEMPLATE_STORE_ALIAS = 'Umb.Store.Template';
export const UMB_TEMPLATE_ITEM_STORE_ALIAS = 'Umb.Store.TemplateItem';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_TEMPLATE_STORE_ALIAS,
	name: 'Template Store',
	api: UmbTemplateStore,
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_TEMPLATE_ITEM_STORE_ALIAS,
	name: 'Template Item Store',
	api: UmbTemplateItemStore,
};

export const manifests = [repository, store, itemStore];
