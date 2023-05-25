import { UmbTemplateRepository } from '../repository/partial-views.repository.js';
import { PARTIAL_VIEW_REPOSITORY_ALIAS } from '../config.js';
import { UmbPartialViewsTreeStore } from './partial-views.tree.store.js';
import { UmbPartialViewsStore } from './partial-views.store.js';
import { ManifestRepository, ManifestStore, ManifestTreeStore } from '@umbraco-cms/backoffice/extension-registry';

const repository: ManifestRepository = {
	type: 'repository',
	alias: PARTIAL_VIEW_REPOSITORY_ALIAS,
	name: 'Partial Views Repository',
	class: UmbTemplateRepository,
};

export const PARTIAL_VIEW_STORE_ALIAS = 'Umb.Store.PartialViews';
export const PARTIAL_VIEW_TREE_STORE_ALIAS = 'Umb.Store.PartialViewsTree';

const store: ManifestStore = {
	type: 'store',
	alias: PARTIAL_VIEW_STORE_ALIAS,
	name: 'Partial Views Store',
	class: UmbPartialViewsStore,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: PARTIAL_VIEW_TREE_STORE_ALIAS,
	name: 'Partial Views Tree Store',
	class: UmbPartialViewsTreeStore,
};

export const manifests = [repository, store, treeStore];
