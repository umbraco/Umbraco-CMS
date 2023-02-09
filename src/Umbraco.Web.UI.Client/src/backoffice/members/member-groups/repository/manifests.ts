import { UmbMemberGroupRepository } from './member-group.repository';
import { ManifestRepository } from 'libs/extensions-registry/repository.models';

export const MEMBER_GROUP_REPOSITORY_ALIAS = 'Umb.Repository.MemberGroup';

const repository: ManifestRepository = {
	type: 'repository',
	alias: MEMBER_GROUP_REPOSITORY_ALIAS,
	name: 'Member Group Repository',
	class: UmbMemberGroupRepository,
};

export const manifests = [repository];
