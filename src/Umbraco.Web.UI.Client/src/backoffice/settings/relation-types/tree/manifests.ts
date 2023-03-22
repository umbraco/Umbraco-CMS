import { UmbRelationTypeRepository } from '../repository/relation-type.repository';
import type { ManifestTree } from '@umbraco-cms/backoffice/extensions-registry';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.RelationTypes',
	name: 'Relation Types Tree',
	meta: {
		repository: UmbRelationTypeRepository,
	},
};

export const manifests = [tree];
