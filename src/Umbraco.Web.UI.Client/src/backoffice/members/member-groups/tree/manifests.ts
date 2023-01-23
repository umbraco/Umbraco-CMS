import { STORE_ALIAS } from '../member-group.store';
import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';

const treeAlias = 'Umb.Tree.MemberGroups';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Member Groups Tree',
	meta: {
		storeAlias: STORE_ALIAS,
	},
};

const treeItemActions: Array<ManifestTreeItemAction> = [];

export const manifests = [tree, ...treeItemActions];
