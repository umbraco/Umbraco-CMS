import { DOCUMENT_REPOSITORY_ALIAS } from '../repository/manifests.js';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extension-registry';

export const DOCUMENT_TREE_ALIAS = 'Umb.Tree.Documents';

const tree: ManifestTree = {
	type: 'tree',
	alias: DOCUMENT_TREE_ALIAS,
	name: 'Documents Tree',
	meta: {
		repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	alias: 'Umb.TreeItem.Document',
	name: 'Document Tree Item',
	loader: () => import('./tree-item/document-tree-item.element.js'),
	conditions: {
		entityTypes: ['document-root', 'document'],
	},
};

export const manifests = [tree, treeItem];
