import { UMB_LANGUAGE_ITEM_REPOSITORY_ALIAS, UMB_LANGUAGE_STORE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_LANGUAGE_ITEM_REPOSITORY_ALIAS,
		name: 'Language Item Repository',
		api: () => import('./language-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_LANGUAGE_STORE_ALIAS,
		name: 'Language Item Store',
		api: () => import('./language-item.store.js'),
	},
];
