import { UmbTemplateRepository } from '../repository/template.repository.js';
import { UmbTemplateTreeStore } from './template.tree.store.js';
import { UmbTemplateStore } from './template.store.js';
import { UmbTemplateItemStore } from './template-item.store.js';
import {
	ManifestStore,
	ManifestTreeStore,
	ManifestRepository,
	ManifestItemStore,
} from '@umbraco-cms/backoffice/extension-registry';

export const TEMPLATE_REPOSITORY_ALIAS = 'Umb.Repository.Template';

const repository: ManifestRepository = {
	type: 'repository',
	alias: TEMPLATE_REPOSITORY_ALIAS,
	name: 'Template Repository',
	api: UmbTemplateRepository,
};

export const TEMPLATE_STORE_ALIAS = 'Umb.Store.Template';
export const TEMPLATE_TREE_STORE_ALIAS = 'Umb.Store.TemplateTree';
export const TEMPLATE_ITEM_STORE_ALIAS = 'Umb.Store.TemplateItem';

const store: ManifestStore = {
	type: 'store',
	alias: TEMPLATE_STORE_ALIAS,
	name: 'Template Store',
	api: UmbTemplateStore,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: TEMPLATE_TREE_STORE_ALIAS,
	name: 'Template Tree Store',
	api: UmbTemplateTreeStore,
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: TEMPLATE_ITEM_STORE_ALIAS,
	name: 'Template Item Store',
	api: UmbTemplateItemStore,
};

export const manifests = [repository, store, treeStore, itemStore];
