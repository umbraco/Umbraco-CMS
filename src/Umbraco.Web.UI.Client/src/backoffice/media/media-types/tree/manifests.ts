import { UmbMediaTypeRepository } from '../repository/media-type.repository';
import type { ManifestTree } from '@umbraco-cms/models';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.MediaTypes',
	name: 'Media Types Tree',
	meta: {
		repository: UmbMediaTypeRepository,
	},
};

export const manifests = [tree];
