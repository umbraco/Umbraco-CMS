import { STYLESHEET_REPOSITORY_ALIAS, STYLESHEET_TREE_STORE_ALIAS } from '../config.js';
import { UmbStylesheetRepository } from './stylesheet.repository.js';
import { UmbStylesheetTreeStore } from './stylesheet.tree.store.js';
import { ManifestRepository, ManifestTreeStore } from '@umbraco-cms/backoffice/extension-registry';


const repository: ManifestRepository = {
	type: 'repository',
	alias: STYLESHEET_REPOSITORY_ALIAS,
	name: 'Stylesheet Repository',
	class: UmbStylesheetRepository,
};


const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: STYLESHEET_TREE_STORE_ALIAS,
	name: 'Stylesheet Tree Store',
	class: UmbStylesheetTreeStore,
};

export const manifests = [treeStore, repository];
