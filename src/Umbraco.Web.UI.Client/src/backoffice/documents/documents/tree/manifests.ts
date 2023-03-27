import { DOCUMENT_REPOSITORY_ALIAS } from '../repository/manifests';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extensions-registry';

const treeAlias = 'Umb.Tree.Documents';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Documents Tree',
	meta: {
		repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	alias: 'Umb.TreeItem.Document',
	name: 'Document Tree Item',
	loader: () => import('./tree-item/document-tree-item.element'),
	conditions: {
		entityType: 'document',
	},
};

export const manifests = [tree, treeItem];
