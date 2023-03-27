import { MEMBER_GROUP_REPOSITORY_ALIAS } from '../repository/manifests';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extensions-registry';

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
		entityType: 'member-group',
	},
};

export const manifests = [tree, treeItem];
