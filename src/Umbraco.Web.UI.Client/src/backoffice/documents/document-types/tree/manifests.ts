import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.DocumentType',
	name: 'Document Types Tree',
	weight: 400,
	meta: {
		storeAlias: 'umbDocumentTypeStore',
	},
};

const treeItemActions: Array<ManifestTreeItemAction> = [];

export const manifests = [tree, ...treeItemActions];
