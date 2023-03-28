import { MEDIA_TYPE_REPOSITORY_ALIAS } from '../repository/manifests';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extensions-registry';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.MediaTypes',
	name: 'Media Types Tree',
	meta: {
		repositoryAlias: MEDIA_TYPE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'entity',
	alias: 'Umb.TreeItem.MediaType',
	name: 'Media Type Tree Item',
	conditions: {
		entityType: 'media-type',
	},
};

export const manifests = [tree, treeItem];
