import {
	UMB_CURRENT_USER_STORE_ALIAS,
	UMB_CURRENT_USER_REPOSITORY_ALIAS,
	UMB_CURRENT_USER_CONFIG_REPOSITORY_ALIAS,
	UMB_CURRENT_USER_CONFIG_STORE_ALIAS,
} from './constants.js';
import { UmbCurrentUserStore } from './current-user.store.js';
import { UmbCurrentUserConfigStore } from './current-user-config.store.js';

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
		api: UmbCurrentUserStore,
	},
	{
		type: 'store',
		alias: UMB_CURRENT_USER_CONFIG_STORE_ALIAS,
		name: 'Current User Config Store',
		api: UmbCurrentUserConfigStore,
	},
	{
		type: 'repository',
		alias: UMB_CURRENT_USER_CONFIG_REPOSITORY_ALIAS,
		name: 'Current User Config Repository',
		api: () => import('./current-user-config.repository.js'),
	},
];
