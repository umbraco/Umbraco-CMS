import { UMB_TEMPLATE_TREE_STORE_CONTEXT_TOKEN } from './template.tree.store';
import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.Templates',
	name: 'Templates Tree',
	meta: {
		storeAlias: UMB_TEMPLATE_TREE_STORE_CONTEXT_TOKEN.toString(),
	},
};

const treeItemActions: Array<ManifestTreeItemAction> = [];

export const manifests = [tree, ...treeItemActions];
