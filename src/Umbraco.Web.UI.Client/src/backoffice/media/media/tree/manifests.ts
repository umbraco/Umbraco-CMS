import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';

const treeAlias = 'Umb.Tree.Media';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Media Tree',
	weight: 100,
	meta: {
		label: 'Media',
		icon: 'umb:folder',
		sections: ['Umb.Section.Media'],
		storeContextAlias: 'umbMediaStore',
	},
};

const treeItemActions: Array<ManifestTreeItemAction> = [];

export const manifests = [tree, ...treeItemActions];
