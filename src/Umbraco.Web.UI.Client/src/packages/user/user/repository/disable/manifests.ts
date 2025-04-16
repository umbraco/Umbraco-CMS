import { UMB_DISABLE_USER_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DISABLE_USER_REPOSITORY_ALIAS,
		name: 'Disable User Repository',
		api: () => import('./disable-user.repository.js'),
	},
];
