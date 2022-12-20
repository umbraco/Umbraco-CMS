import { ManifestTree } from '@umbraco-cms/extensions-registry';

const treeAlias = 'Umb.Tree.Extensions';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Extensions Tree',
	weight: 500,
	meta: {
		label: 'Extensions',
		icon: 'umb:favorite',
		sections: ['Umb.Section.Settings'],
		rootNodeEntityType: 'extension-root', // TODO: how do we want to handle 'single node trees'. Trees without any children but still needs to open an workspace? Currently an workspace is chosen based on the entity type. The tree root node doesn't have one, so we need to tell which workspace to use.
	},
};

export const manifests = [tree];
