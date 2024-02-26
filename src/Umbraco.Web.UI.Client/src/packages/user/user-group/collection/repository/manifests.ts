import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_USER_GROUP_COLLECTION_REPOSITORY_ALIAS = 'Umb.Repository.UserGroupCollection';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_USER_GROUP_COLLECTION_REPOSITORY_ALIAS,
	name: 'User Group Collection Repository',
	api: () => import('./user-group-collection.repository.js'),
};

export const manifests = [repository];
