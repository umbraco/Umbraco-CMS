import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';

const treeAlias = 'Umb.Tree.Dictionary';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Dictionary Tree',
	weight: 100,
	meta: {
		label: 'Dictionary',
		icon: 'umb:folder',
		sections: ['Umb.Section.Translation'],
		storeContextAlias: 'umbDictionaryStore',
	},
};

const treeItemActions: Array<ManifestTreeItemAction> = [];

export const manifests = [tree, ...treeItemActions];
