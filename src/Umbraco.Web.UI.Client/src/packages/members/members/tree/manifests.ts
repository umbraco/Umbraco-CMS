import { MEMBER_REPOSITORY_ALIAS } from '../repository/manifests';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extension-registry';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.Members',
	name: 'Members Tree',
	weight: 10,
	meta: {
		repositoryAlias: MEMBER_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'entity',
	alias: 'Umb.TreeItem.Member',
	name: 'Member Tree Item',
	conditions: {
		entityTypes: ['member-root', 'member'],
	},
};

export const manifests = [tree, treeItem];
