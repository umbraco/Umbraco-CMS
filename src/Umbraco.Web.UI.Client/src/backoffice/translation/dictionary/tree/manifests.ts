import { DICTIONARY_REPOSITORY_ALIAS } from '../repository/manifests';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extensions-registry';

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
	conditions: {
		entityType: 'dictionary-item',
	},
};

export const manifests = [tree, treeItem];
