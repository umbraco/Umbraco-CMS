import { UmbMemberRepository } from '../repository/member.repository';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extensions-registry';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.Members',
	name: 'Members Tree',
	weight: 10,
	meta: {
		repository: UmbMemberRepository,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'entity',
	alias: 'Umb.TreeItem.Member',
	name: 'Member Tree Item',
	conditions: {
		entityType: 'member',
	},
};

export const manifests = [tree, treeItem];
