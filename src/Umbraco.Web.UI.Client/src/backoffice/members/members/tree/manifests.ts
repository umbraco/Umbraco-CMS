import { UmbMemberTreeRepository } from './data/member.tree.repository';
import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.Members',
	name: 'Members Tree',
	weight: 10,
	meta: {
		repository: UmbMemberTreeRepository
	},
};

const treeItemActions: Array<ManifestTreeItemAction> = [	
	{
		type: 'treeItemAction',
		alias: 'Umb.TreeItemAction.Member.Delete',
		name: 'Member Tree Item Action Delete',
		loader: () => import('./actions/action-member-delete.element'),
		weight: 100,
		meta: {
			entityType: 'member',
			label: 'Delete',
			icon: 'delete',
		},
	},
];

export const manifests = [tree, ...treeItemActions];