import { UMB_USER_AVATAR_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_USER_AVATAR_REPOSITORY_ALIAS,
		name: 'User Avatar Repository',
		api: () => import('./user-avatar.repository.js'),
	},
];
