import { UMB_USER_AVATAR_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const avatarRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_USER_AVATAR_REPOSITORY_ALIAS,
	name: 'User Avatar Repository',
	api: () => import('./user-avatar.repository.js'),
};

export const manifests: Array<ManifestTypes> = [avatarRepository];
