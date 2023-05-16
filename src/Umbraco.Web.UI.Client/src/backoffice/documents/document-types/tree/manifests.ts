import { DOCUMENT_TYPE_REPOSITORY_ALIAS } from '../repository/manifests';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extension-registry';

export const DOCUMENT_TYPE_TREE_ALIAS = 'Umb.Tree.DocumentTypes';

const tree: ManifestTree = {
	type: 'tree',
	alias: DOCUMENT_TYPE_TREE_ALIAS,
	name: 'Document Types Tree',
	meta: {
		repositoryAlias: DOCUMENT_TYPE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'entity',
	alias: 'Umb.TreeItem.DocumentType',
	name: 'Document Type Tree Item',
	conditions: {
		entityTypes: ['document-type-root', 'document-type'],
	},
};

export const manifests = [tree, treeItem];
