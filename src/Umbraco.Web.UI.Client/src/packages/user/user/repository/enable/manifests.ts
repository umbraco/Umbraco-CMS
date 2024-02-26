import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_ENABLE_USER_REPOSITORY_ALIAS = 'Umb.Repository.User.Enable';
const enableRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_ENABLE_USER_REPOSITORY_ALIAS,
	name: 'Disable User Repository',
	api: () => import('./enable-user.repository.js'),
};

export const manifests = [enableRepository];
