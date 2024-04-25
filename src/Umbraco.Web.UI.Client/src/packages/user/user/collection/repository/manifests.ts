import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_USER_COLLECTION_REPOSITORY_ALIAS = 'Umb.Repository.UserCollection';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_USER_COLLECTION_REPOSITORY_ALIAS,
	name: 'User Collection Repository',
	api: () => import('./user-collection.repository.js'),
};

export const manifests: Array<ManifestTypes> = [repository];
