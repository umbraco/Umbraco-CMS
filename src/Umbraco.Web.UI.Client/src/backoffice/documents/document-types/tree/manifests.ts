import { UmbDocumentTypeRepository } from '../repository/document-type.repository';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extensions-registry';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.DocumentTypes',
	name: 'Document Types Tree',
	meta: {
		repository: UmbDocumentTypeRepository,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'entity',
	alias: 'Umb.TreeItem.DocumentType',
	name: 'Document Type Tree Item',
	conditions: {
		entityType: 'document-type',
	},
};

export const manifests = [tree, treeItem];
