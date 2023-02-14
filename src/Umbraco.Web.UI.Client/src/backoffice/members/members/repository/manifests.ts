import { UmbMemberRepository } from './member.repository';
import { ManifestRepository } from 'libs/extensions-registry/repository.models';

export const MEMBER_REPOSITORY_ALIAS = 'Umb.Repository.Member';

const repository: ManifestRepository = {
	type: 'repository',
	alias: MEMBER_REPOSITORY_ALIAS,
	name: 'Member Repository',
	class: UmbMemberRepository,
};

export const manifests = [repository];
