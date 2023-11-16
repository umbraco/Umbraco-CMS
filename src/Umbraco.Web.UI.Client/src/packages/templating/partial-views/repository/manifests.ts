import { UmbPartialViewRepository } from '../repository/partial-views.repository.js';
import { UmbPartialViewTreeStore } from './partial-views.tree.store.js';
import { UmbPartialViewStore } from './partial-views.store.js';
import { ManifestRepository, ManifestStore, ManifestTreeStore } from '@umbraco-cms/backoffice/extension-registry';

export const PARTIAL_VIEW_REPOSITORY_ALIAS = 'Umb.Repository.PartialView';

const repository: ManifestRepository = {
	type: 'repository',
	alias: PARTIAL_VIEW_REPOSITORY_ALIAS,
	name: 'Partial View Repository',
	api: UmbPartialViewRepository,
};

export const PARTIAL_VIEW_STORE_ALIAS = 'Umb.Store.PartialView';
export const PARTIAL_VIEW_TREE_STORE_ALIAS = 'Umb.Store.PartialViewTree';

const store: ManifestStore = {
	type: 'store',
	alias: PARTIAL_VIEW_STORE_ALIAS,
	name: 'Partial View Store',
	api: UmbPartialViewStore,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: PARTIAL_VIEW_TREE_STORE_ALIAS,
	name: 'Partial View Tree Store',
	api: UmbPartialViewTreeStore,
};

export const manifests = [repository, store, treeStore];
