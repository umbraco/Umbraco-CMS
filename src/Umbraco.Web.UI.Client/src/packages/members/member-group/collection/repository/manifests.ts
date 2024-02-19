import { UmbMemberGroupCollectionRepository } from './member-group-collection.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEMBER_GROUP_COLLECTION_REPOSITORY_ALIAS = 'Umb.Repository.MemberGroup.Collection';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEMBER_GROUP_COLLECTION_REPOSITORY_ALIAS,
	name: 'Member Group Collection Repository',
	api: UmbMemberGroupCollectionRepository,
};

export const manifests = [repository];
