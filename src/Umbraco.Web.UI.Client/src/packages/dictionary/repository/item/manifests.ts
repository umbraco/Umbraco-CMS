import { UMB_DICTIONARY_ITEM_REPOSITORY_ALIAS, UMB_DICTIONARY_STORE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DICTIONARY_ITEM_REPOSITORY_ALIAS,
		name: 'Dictionary Item Repository',
		api: () => import('./dictionary-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_DICTIONARY_STORE_ALIAS,
		name: 'Dictionary Item Store',
		api: () => import('./dictionary-item.store.js'),
	},
];
