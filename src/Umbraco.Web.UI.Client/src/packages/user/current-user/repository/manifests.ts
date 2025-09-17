import {
	UMB_CURRENT_USER_STORE_ALIAS,
	UMB_CURRENT_USER_REPOSITORY_ALIAS,
	UMB_CURRENT_USER_CONFIG_REPOSITORY_ALIAS,
	UMB_CURRENT_USER_CONFIG_STORE_ALIAS,
} from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_CURRENT_USER_REPOSITORY_ALIAS,
		name: 'Current User Repository',
		api: () => import('./current-user.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_CURRENT_USER_STORE_ALIAS,
		name: 'Current User Store',
		api: () => import('./current-user.store.js'),
	},
	{
		type: 'store',
		alias: UMB_CURRENT_USER_CONFIG_STORE_ALIAS,
		name: 'Current User Config Store',
		api: () => import('./current-user-config.store.js'),
	},
	{
		type: 'repository',
		alias: UMB_CURRENT_USER_CONFIG_REPOSITORY_ALIAS,
		name: 'Current User Config Repository',
		api: () => import('./current-user-config.repository.js'),
	},
];
