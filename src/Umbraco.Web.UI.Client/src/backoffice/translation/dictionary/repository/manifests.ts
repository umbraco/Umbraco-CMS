import { UmbDictionaryRepository } from '../repository/dictionary.repository';
import { UmbDictionaryTreeStore } from './dictionary.tree.store';
import { UmbDictionaryStore } from './dictionary.store';
import { ManifestStore, ManifestTreeStore, ManifestRepository } from '@umbraco-cms/backoffice/extensions-registry';

export const DICTIONARY_REPOSITORY_ALIAS = 'Umb.Repository.Dictionary';

const repository: ManifestRepository = {
	type: 'repository',
	alias: DICTIONARY_REPOSITORY_ALIAS,
	name: 'Dictionary Repository',
	class: UmbDictionaryRepository,
};

export const DICTIONARY_STORE_ALIAS = 'Umb.Store.Dictionary';
export const DICTIONARY_TREE_STORE_ALIAS = 'Umb.Store.DictionaryTree';

const store: ManifestStore = {
	type: 'store',
	alias: DICTIONARY_STORE_ALIAS,
	name: 'Dictionary Store',
	class: UmbDictionaryStore,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: DICTIONARY_TREE_STORE_ALIAS,
	name: 'Dictionary Tree Store',
	class: UmbDictionaryTreeStore,
};

export const manifests = [repository, store, treeStore];
