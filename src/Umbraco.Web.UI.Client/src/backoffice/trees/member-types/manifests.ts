import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';

const treeAlias = 'Umb.Tree.MemberTypes';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Member Types Tree',
	weight: 200,
	meta: {
		label: 'Member Types',
		icon: 'umb:folder',
		sections: ['Umb.Section.Settings'],
		storeContextAlias: 'umbMemberTypeStore',
	},
};

const treeItemActions: Array<ManifestTreeItemAction> = [];

export const manifests = [tree, ...treeItemActions];
