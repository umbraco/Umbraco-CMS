import { DICTIONARY_REPOSITORY_ALIAS } from '../repository/manifests.js';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extension-registry';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.Dictionary',
	name: 'Dictionary Tree',
	meta: {
		repositoryAlias: DICTIONARY_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'entity',
	alias: 'Umb.TreeItem.DictionaryItem',
	name: 'Dictionary Item Tree Item',
	meta: {
		entityTypes: ['dictionary-root', 'dictionary-item'],
	},
};

export const manifests = [tree, treeItem];
