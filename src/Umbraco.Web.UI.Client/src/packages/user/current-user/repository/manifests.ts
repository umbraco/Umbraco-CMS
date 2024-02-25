import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_CURRENT_USER_REPOSITORY_ALIAS = 'Umb.Repository.CurrentUser';

const avatarRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_CURRENT_USER_REPOSITORY_ALIAS,
	name: 'Current User Repository',
	api: () => import('./current-user.repository.js'),
};

export const manifests = [avatarRepository];
