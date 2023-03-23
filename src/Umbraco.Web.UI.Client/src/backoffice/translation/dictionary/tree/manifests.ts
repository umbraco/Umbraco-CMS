import { UmbDictionaryRepository } from '../repository/dictionary.repository';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extensions-registry';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.Dictionary',
	name: 'Dictionary Tree',
	meta: {
		repository: UmbDictionaryRepository,
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
