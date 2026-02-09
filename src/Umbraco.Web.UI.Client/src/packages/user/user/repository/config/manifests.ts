import { UMB_USER_CONFIG_REPOSITORY_ALIAS, UMB_USER_CONFIG_STORE_ALIAS } from './constants.js';
import { UmbUserConfigStore } from './user-config.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'store',
		alias: UMB_USER_CONFIG_STORE_ALIAS,
		name: 'User Config Store',
		api: UmbUserConfigStore,
	},
	{
		type: 'repository',
		alias: UMB_USER_CONFIG_REPOSITORY_ALIAS,
		name: 'User Config Repository',
		api: () => import('./user-config.repository.js'),
	},
];
