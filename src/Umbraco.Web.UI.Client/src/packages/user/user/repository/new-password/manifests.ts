import { UMB_NEW_USER_PASSWORD_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_NEW_USER_PASSWORD_REPOSITORY_ALIAS,
		name: 'New User Password Repository',
		api: () => import('./new-user-password.repository.js'),
	},
];
