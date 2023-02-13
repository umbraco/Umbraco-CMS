import { UMB_MEDIA_TYPE_TREE_STORE_CONTEXT_TOKEN } from '../media-type.tree.store';
import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.MediaTypes',
	name: 'Media Types Tree',
	meta: {
		storeAlias: UMB_MEDIA_TYPE_TREE_STORE_CONTEXT_TOKEN.toString(),
	},
};

const treeItemActions: Array<ManifestTreeItemAction> = [];

export const manifests = [tree, ...treeItemActions];
