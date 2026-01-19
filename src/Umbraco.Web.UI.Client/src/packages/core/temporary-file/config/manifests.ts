import { UMB_TEMPORARY_FILE_CONFIG_STORE_ALIAS, UMB_TEMPORARY_FILE_REPOSITORY_ALIAS } from './constants.js';
import { UmbTemporaryFileConfigStore } from './config.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'store',
		alias: UMB_TEMPORARY_FILE_CONFIG_STORE_ALIAS,
		name: 'Temporary File Config Store',
		api: UmbTemporaryFileConfigStore,
	},
	{
		type: 'repository',
		alias: UMB_TEMPORARY_FILE_REPOSITORY_ALIAS,
		name: 'Temporary File Config Repository',
		api: () => import('./config.repository.js'),
	},
];
