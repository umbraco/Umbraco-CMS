import { UmbUserAvatarRepository } from './user-avatar.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_USER_AVATAR_REPOSITORY_ALIAS = 'Umb.Repository.User.Avatar';

const avatarRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_USER_AVATAR_REPOSITORY_ALIAS,
	name: 'User Avatar Repository',
	api: UmbUserAvatarRepository,
};

export const manifests = [avatarRepository];
