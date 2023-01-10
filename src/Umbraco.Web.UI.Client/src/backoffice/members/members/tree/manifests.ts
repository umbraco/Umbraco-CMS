import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.Members',
	name: 'Members Tree',
	weight: 100,
	meta: {
		label: 'Members',
		icon: 'umb:folder',
		sections: ['Umb.Section.Members'],
		storeAlias: 'umbMemberTypesStore',
	},
};

const treeItemActions: Array<ManifestTreeItemAction> = [];

export const manifests = [tree, ...treeItemActions];
