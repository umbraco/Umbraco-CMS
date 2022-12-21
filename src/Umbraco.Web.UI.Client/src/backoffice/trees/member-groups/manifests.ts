import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';

const treeAlias = 'Umb.Tree.MemberGroups';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Member Groups Tree',
	weight: 1,
	meta: {
		label: 'Member Groups',
		icon: 'umb:folder',
		sections: ['Umb.Section.Members'],
		storeContextAlias: 'umbMemberGroupStore',
	},
};

const treeItemActions: Array<ManifestTreeItemAction> = [];

export const manifests = [tree, ...treeItemActions];
