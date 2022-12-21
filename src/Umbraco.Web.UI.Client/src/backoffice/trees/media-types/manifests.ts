import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';

const treeAlias = 'Umb.Tree.MediaTypes';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Media Types Tree',
	weight: 200,
	meta: {
		label: 'Media Types',
		icon: 'umb:folder',
		sections: ['Umb.Section.Settings'],
		storeContextAlias: 'umbMediaTypeStore',
	},
};

const treeItemActions: Array<ManifestTreeItemAction> = [];

export const manifests = [tree, ...treeItemActions];
