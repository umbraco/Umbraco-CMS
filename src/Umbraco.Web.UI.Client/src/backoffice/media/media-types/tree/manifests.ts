import { UmbMediaTypeRepository } from '../repository/media-type.repository';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extensions-registry';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.MediaTypes',
	name: 'Media Types Tree',
	meta: {
		repository: UmbMediaTypeRepository,
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
