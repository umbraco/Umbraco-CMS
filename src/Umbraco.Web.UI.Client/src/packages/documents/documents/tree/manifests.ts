import { DOCUMENT_REPOSITORY_ALIAS } from '../repository/manifests.js';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_TREE_ALIAS = 'Umb.Tree.Document';

const tree: ManifestTree = {
	type: 'tree',
	alias: UMB_DOCUMENT_TREE_ALIAS,
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
	meta: {
		entityTypes: ['document-root', 'document'],
	},
};

export const manifests = [tree, treeItem];
