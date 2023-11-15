import { MEDIA_REPOSITORY_ALIAS } from '../repository/manifests.js';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extension-registry';

const UMB_MEDIA_TREE_ALIAS = 'Umb.Tree.Media';

const tree: ManifestTree = {
	type: 'tree',
	alias: UMB_MEDIA_TREE_ALIAS,
	name: 'Media Tree',
	meta: {
		repositoryAlias: MEDIA_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'entity',
	alias: 'Umb.TreeItem.Media',
	name: 'Media Tree Item',
	meta: {
		entityTypes: ['media-root', 'media'],
	},
};

export const manifests = [tree, treeItem];
