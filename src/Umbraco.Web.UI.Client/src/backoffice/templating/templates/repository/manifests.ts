import { UmbTemplateRepository } from '../repository/template.repository';
import { UmbTemplateTreeStore } from './template.tree.store';
import { UmbTemplateStore } from './template.store';
import { ManifestRepository } from 'libs/extensions-registry/repository.models';
import { ManifestStore, ManifestTreeStore } from '@umbraco-cms/extensions-registry';

export const TEMPLATE_REPOSITORY_ALIAS = 'Umb.Repository.Template';

const repository: ManifestRepository = {
	type: 'repository',
	alias: TEMPLATE_REPOSITORY_ALIAS,
	name: 'Template Repository',
	class: UmbTemplateRepository,
};

export const TEMPLATE_STORE_ALIAS = 'Umb.Store.Template';
export const TEMPLATE_TREE_STORE_ALIAS = 'Umb.Store.TemplateTree';

const store: ManifestStore = {
	type: 'store',
	alias: TEMPLATE_STORE_ALIAS,
	name: 'Template Store',
	class: UmbTemplateStore,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: TEMPLATE_TREE_STORE_ALIAS,
	name: 'Template Tree Store',
	class: UmbTemplateTreeStore,
};

export const manifests = [repository, store, treeStore];
