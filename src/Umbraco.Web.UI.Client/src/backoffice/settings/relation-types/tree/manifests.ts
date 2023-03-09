import { UmbRelationTypeRepository } from '../repository/relation-type.repository';
import type { ManifestTree } from '@umbraco-cms/models';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.RelationTypes',
	name: 'Data Types Tree',
	meta: {
		repository: UmbRelationTypeRepository,
	},
};

export const manifests = [tree];
