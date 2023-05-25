import { MEMBER_GROUP_REPOSITORY_ALIAS } from '../repository/manifests.js';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extension-registry';

const treeAlias = 'Umb.Tree.MemberGroups';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Member Groups Tree',
	weight: 100,
	meta: {
		repositoryAlias: MEMBER_GROUP_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'entity',
	alias: 'Umb.TreeItem.MemberGroup',
	name: 'Member Group Tree Item',
	conditions: {
		entityTypes: ['member-group-root', 'member-group'],
	},
};

export const manifests = [tree, treeItem];
