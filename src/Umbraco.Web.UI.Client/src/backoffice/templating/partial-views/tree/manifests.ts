import { PARTIAL_VIEW_ENTITY_TYPE } from '..';
import { PARTIAL_VIEW_REPOSITORY_ALIAS } from '../repository/manifests';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extensions-registry';

export const PARTIAL_VIEW_TREE_ALIAS = 'Umb.Tree.PartialViews';

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
		entityType: PARTIAL_VIEW_ENTITY_TYPE,
	},
};

export const manifests = [tree, treeItem];
