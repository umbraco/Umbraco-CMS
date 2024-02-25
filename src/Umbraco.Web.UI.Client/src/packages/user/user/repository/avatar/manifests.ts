import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_USER_AVATAR_REPOSITORY_ALIAS = 'Umb.Repository.User.Avatar';

const avatarRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_USER_AVATAR_REPOSITORY_ALIAS,
	name: 'User Avatar Repository',
	api: () => import('./user-avatar.repository.js'),
};

export const manifests = [avatarRepository];
