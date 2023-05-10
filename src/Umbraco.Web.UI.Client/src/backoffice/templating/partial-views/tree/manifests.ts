import { PARTIAL_VIEW_ENTITY_TYPE, PARTIAL_VIEW_REPOSITORY_ALIAS, PARTIAL_VIEW_TREE_ALIAS } from '../config';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extensions-registry';

const tree: ManifestTree = {
	type: 'tree',
	alias: PARTIAL_VIEW_TREE_ALIAS,
	name: 'Partial Views Tree',
	meta: {
		repositoryAlias: PARTIAL_VIEW_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'fileSystem',
	alias: 'Umb.TreeItem.PartialViews',
	name: 'Partial Views Tree Item',
	conditions: {
		entityTypes: [PARTIAL_VIEW_ENTITY_TYPE],
	},
};

export const manifests = [tree, treeItem];
