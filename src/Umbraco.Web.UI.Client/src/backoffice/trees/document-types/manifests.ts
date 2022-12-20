import type { ManifestTree } from '@umbraco-cms/extensions-registry';

const treeAlias = 'Umb.Tree.DocumentTypes';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Document Types Tree',
	weight: 400,
	meta: {
		label: 'Document Types',
		icon: 'umb:folder',
		sections: ['Umb.Section.Settings'],
		storeContextAlias: 'umbDocumentTypeStore',
	},
};

export const manifests = [tree];
