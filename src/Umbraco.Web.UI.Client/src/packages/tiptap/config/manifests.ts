import { UMB_TIPTAP_UMBRACO_PATH_CONFIG_STORE_ALIAS, UMB_TIPTAP_UMBRACO_PATH_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'store',
		alias: UMB_TIPTAP_UMBRACO_PATH_CONFIG_STORE_ALIAS,
		name: 'Tiptap Umbraco Path Config Store',
		api: () => import('./config.store.js'),
	},
	{
		type: 'repository',
		alias: UMB_TIPTAP_UMBRACO_PATH_REPOSITORY_ALIAS,
		name: 'Tiptap Umbraco Path Config Repository',
		api: () => import('./config.repository.js'),
	},
];
