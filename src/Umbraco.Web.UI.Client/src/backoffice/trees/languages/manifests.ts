import type { ManifestTree } from '@umbraco-cms/models';

const treeAlias = 'Umb.Tree.Languages';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Languages Tree',
	weight: 100,
	meta: {
		label: 'Languages',
		icon: 'umb:globe',
		sections: ['Umb.Section.Settings'],
		rootNodeEntityType: 'language-root', // TODO: how do we want to handle 'single node trees'. Trees without any children but still needs to open an workspace? Currently an workspace is chosen based on the entity type. The tree root node doesn't have one, so we need to tell which workspace to use.
	},
};

export const manifests = [tree];
