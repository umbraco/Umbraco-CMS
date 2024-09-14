import { UMB_ENABLE_USER_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_ENABLE_USER_REPOSITORY_ALIAS,
		name: 'Disable User Repository',
		api: () => import('./enable-user.repository.js'),
	},
];
