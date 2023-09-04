import { SCRIPTS_REPOSITORY_ALIAS } from '../config.js';
import { UmbScriptsRepository } from './scripts.repository.js';
import { UmbScriptsStore } from './scripts.store.js';
import { UmbScriptsTreeStore } from './scripts.tree.store.js';
import { ManifestRepository, ManifestStore, ManifestTreeStore } from '@umbraco-cms/backoffice/extension-registry';

const repository: ManifestRepository = {
	type: 'repository',
	alias: SCRIPTS_REPOSITORY_ALIAS,
	name: 'Partial Views Repository',
	class: UmbScriptsRepository,
};

export const SCRIPTS_STORE_ALIAS = 'Umb.Store.PartialViews';
export const SCRIPTS_TREE_STORE_ALIAS = 'Umb.Store.PartialViewsTree';

const store: ManifestStore = {
	type: 'store',
	alias: SCRIPTS_STORE_ALIAS,
	name: 'Partial Views Store',
	class: UmbScriptsStore,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: SCRIPTS_TREE_STORE_ALIAS,
	name: 'Partial Views Tree Store',
	class: UmbScriptsTreeStore,
};

export const manifests = [repository, store, treeStore];
