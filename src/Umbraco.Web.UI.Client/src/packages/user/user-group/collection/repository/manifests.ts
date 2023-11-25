import { UmbUserGroupCollectionRepository } from './user-group-collection.repository.js';
import { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_USER_GROUP_COLLECTION_REPOSITORY_ALIAS = 'Umb.Repository.UserGroupCollection';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_USER_GROUP_COLLECTION_REPOSITORY_ALIAS,
	name: 'User Group Collection Repository',
	api: UmbUserGroupCollectionRepository,
};

export const manifests = [repository];
