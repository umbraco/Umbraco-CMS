import type { ManifestTree } from '@umbraco-cms/extensions-registry';

const treeAlias = 'Umb.Tree.DocumentBlueprints';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Document Blueprints Tree',
	weight: 400,
	meta: {
		label: 'Document Blueprints',
		icon: 'umb:blueprint',
		sections: ['Umb.Section.Settings'],
		rootNodeEntityType: 'document-blueprint-root',
	},
};

export const manifests = [tree];
