import { DOCUMENT_RECYCLE_BIN_REPOSITORY_ALIAS } from '../repository/manifests.js';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extension-registry';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.DocumentRecycleBin',
	name: 'Document Recycle Bin Tree',
	meta: {
		repositoryAlias: DOCUMENT_RECYCLE_BIN_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'entity',
	alias: 'Umb.TreeItem.DocumentRecycleBin',
	name: 'Document Recycle Bin Tree Item',
	meta: {
		entityTypes: ['document-recycle-bin-root'],
	},
};

export const manifests = [tree, treeItem];
