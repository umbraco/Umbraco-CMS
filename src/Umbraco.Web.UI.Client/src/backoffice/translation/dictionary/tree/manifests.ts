import { UmbDictionaryRepository } from '../repository/dictionary.repository';
import type { ManifestTree } from '@umbraco-cms/models';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.Dictionary',
	name: 'Dictionary Tree',
	meta: {
		repository: UmbDictionaryRepository,
	},
};

export const manifests = [tree];
