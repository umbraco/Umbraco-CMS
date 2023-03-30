import { UmbStylesheetRepository } from './stylesheet.repository';
import { UmbStylesheetTreeStore } from './stylesheet.tree.store';
import { ManifestRepository, ManifestTreeStore } from '@umbraco-cms/backoffice/extensions-registry';

export const STYLESHEET_REPOSITORY_ALIAS = 'Umb.Repository.Stylesheet';

const repository: ManifestRepository = {
	type: 'repository',
	alias: STYLESHEET_REPOSITORY_ALIAS,
	name: 'Stylesheet Repository',
	class: UmbStylesheetRepository,
};

export const STYLESHEET_STORE_ALIAS = 'Umb.Store.Stylesheet';
export const STYLESHEET_TREE_STORE_ALIAS = 'Umb.Store.StylesheetTree';

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: STYLESHEET_TREE_STORE_ALIAS,
	name: 'Stylesheet Tree Store',
	class: UmbStylesheetTreeStore,
};

export const manifests = [treeStore, repository];
