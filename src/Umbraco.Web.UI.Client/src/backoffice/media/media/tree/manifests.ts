import { UmbMediaRepository } from '../repository/media.repository';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extensions-registry';

const treeAlias = 'Umb.Tree.Media';

const tree: ManifestTree = {
	type: 'tree',
	alias: treeAlias,
	name: 'Media Tree',
	meta: {
		repository: UmbMediaRepository, // TODO: use alias instead of class
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
