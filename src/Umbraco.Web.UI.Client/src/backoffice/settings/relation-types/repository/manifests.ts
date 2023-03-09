import { UmbRelationTypeRepository } from '../repository/relation-type.repository';
import { ManifestRepository } from 'libs/extensions-registry/repository.models';

export const RELATION_TYPE_REPOSITORY_ALIAS = 'Umb.Repository.RelationTypes';

const repository: ManifestRepository = {
	type: 'repository',
	alias: RELATION_TYPE_REPOSITORY_ALIAS,
	name: 'Data Types Repository',
	class: UmbRelationTypeRepository,
};

export const manifests = [repository];
