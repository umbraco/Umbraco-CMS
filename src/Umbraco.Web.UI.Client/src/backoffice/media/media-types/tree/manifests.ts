import { UmbMediaTypeRepository } from '../repository/media-type.repository';
import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.MediaTypes',
	name: 'Media Types Tree',
	meta: {
		repository: UmbMediaTypeRepository,
	},
};

const treeItemActions: Array<ManifestTreeItemAction> = [];

export const manifests = [tree, ...treeItemActions];
