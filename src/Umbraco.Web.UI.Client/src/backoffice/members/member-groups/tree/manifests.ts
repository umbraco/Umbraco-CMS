import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';
import { UMB_MEMBER_GROUP_TREE_STORE_CONTEXT_TOKEN } from '../member-group.tree.store';

const treeAlias = 'Umb.Tree.MemberGroups';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Member Groups Tree',
	weight: 100,
	meta: {
		storeAlias: UMB_MEMBER_GROUP_TREE_STORE_CONTEXT_TOKEN.toString(),
	},
};

const treeItemActions: Array<ManifestTreeItemAction> = [];

export const manifests = [tree, ...treeItemActions];
