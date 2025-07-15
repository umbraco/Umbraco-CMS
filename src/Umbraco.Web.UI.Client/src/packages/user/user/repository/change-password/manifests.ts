import { UMB_CHANGE_USER_PASSWORD_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_CHANGE_USER_PASSWORD_REPOSITORY_ALIAS,
		name: 'Change User Password Repository',
		api: () => import('./change-user-password.repository.js'),
	},
];
