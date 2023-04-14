import { MEDIA_REPOSITORY_ALIAS } from '../repository/manifests';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extensions-registry';

const treeAlias = 'Umb.Tree.Media';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
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
	conditions: {
		entityType: 'media',
	},
};

export const manifests = [tree, treeItem];
