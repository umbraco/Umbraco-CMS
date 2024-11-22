import { UMB_USER_CONFIG_REPOSITORY_ALIAS, UMB_USER_CONFIG_STORE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'store',
		alias: UMB_USER_CONFIG_STORE_ALIAS,
		name: 'User Config Store',
		api: () => import('./user-config.store.js'),
	},
	{
		type: 'repository',
		alias: UMB_USER_CONFIG_REPOSITORY_ALIAS,
		name: 'User Config Repository',
		api: () => import('./user-config.repository.js'),
	},
];
