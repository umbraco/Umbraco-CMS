import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.DocumentBlueprint',
	name: 'Document Blueprints Tree',
	weight: 400,
	meta: {
		label: 'Document Blueprints',
		icon: 'umb:blueprint',
		sections: ['Umb.Section.Settings'],
		rootNodeEntityType: 'document-blueprint-root',
	},
};

const treeItemActions: Array<ManifestTreeItemAction> = [];

export const manifests = [tree, ...treeItemActions];
