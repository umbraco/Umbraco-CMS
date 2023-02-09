import { UmbMemberGroupTreeRepository } from './data/member-group.tree.repository';
import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';

const treeAlias = 'Umb.Tree.MemberGroups';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Member Groups Tree',
	weight: 100,
	meta: {
		repository: UmbMemberGroupTreeRepository
	},
};

const treeItemActions: Array<ManifestTreeItemAction> = [	
	{
		type: 'treeItemAction',
		alias: 'Umb.TreeItemAction.MemberGroup.Delete',
		name: 'Member Group Tree Item Action Delete',
		loader: () => import('./actions/action-member-group-delete.element'),
		weight: 100,
		meta: {
			entityType: 'member-group',
			label: 'Delete',
			icon: 'delete',
		},
	},
];

export const manifests = [tree, ...treeItemActions];
