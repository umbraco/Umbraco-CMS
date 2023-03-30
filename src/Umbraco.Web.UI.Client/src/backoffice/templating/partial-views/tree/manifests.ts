import { PARTIAL_VIEW_REPOSITORY_ALIAS } from '../repository/manifests';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extensions-registry';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.PartialViews',
	name: 'Partial Views Tree',
	meta: {
		repositoryAlias: PARTIAL_VIEW_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'entity',
	alias: 'Umb.TreeItem.PartialViews',
	name: 'Partial Views Tree Item',
	conditions: {
		entityType: 'partial-view',
	},
};

export const manifests = [tree, treeItem];
