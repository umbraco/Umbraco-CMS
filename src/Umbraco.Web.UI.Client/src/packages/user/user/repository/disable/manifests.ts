import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DISABLE_USER_REPOSITORY_ALIAS = 'Umb.Repository.User.Disable';
const disableRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DISABLE_USER_REPOSITORY_ALIAS,
	name: 'Disable User Repository',
	api: () => import('./disable-user.repository.js'),
};

export const manifests = [disableRepository];
