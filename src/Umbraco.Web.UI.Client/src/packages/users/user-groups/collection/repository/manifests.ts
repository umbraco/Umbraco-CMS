import { UmbUserGroupCollectionRepository } from './user-group-collection.repository.js';
import { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const USER_GROUP_COLLECTION_REPOSITORY_ALIAS = 'Umb.Repository.UserGroupCollection';

const repository: ManifestRepository = {
	type: 'repository',
	alias: USER_GROUP_COLLECTION_REPOSITORY_ALIAS,
	name: 'User Group Collection Repository',
	class: UmbUserGroupCollectionRepository,
};

export const manifests = [repository];
