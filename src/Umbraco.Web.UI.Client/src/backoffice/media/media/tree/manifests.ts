import { UMB_MEDIA_TREE_STORE_CONTEXT_TOKEN } from '../media.tree.store';
import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';

const treeAlias = 'Umb.Tree.Media';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Media Tree',
	meta: {
		storeAlias: UMB_MEDIA_TREE_STORE_CONTEXT_TOKEN.toString(),
	},
};

const treeItemActions: Array<ManifestTreeItemAction> = [];

export const manifests = [tree, ...treeItemActions];
