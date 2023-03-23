import { UmbDocumentRepository } from '../repository/document.repository';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extensions-registry';

const treeAlias = 'Umb.Tree.Documents';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Documents Tree',
	meta: {
		repository: UmbDocumentRepository, // TODO: use alias instead of class
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'entity',
	alias: 'Umb.TreeItem.Document',
	name: 'Document Tree Item',
	conditions: {
		entityType: 'document',
	},
};

export const manifests = [tree, treeItem];
