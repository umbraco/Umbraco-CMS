import { UmbMemberTypeRepository } from './member-type.repository';
import { ManifestRepository } from 'libs/extensions-registry/repository.models';

export const MEMBER_TYPES_REPOSITORY_ALIAS = 'Umb.Repository.MemberTypes';

const repository: ManifestRepository = {
	type: 'repository',
	alias: MEMBER_TYPES_REPOSITORY_ALIAS,
	name: 'Member Types Repository',
	class: UmbMemberTypeRepository,
};

export const manifests = [repository];