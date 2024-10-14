import { UMB_DICTIONARY_DETAIL_REPOSITORY_ALIAS, UMB_DICTIONARY_DETAIL_STORE_ALIAS } from './constants.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DICTIONARY_DETAIL_REPOSITORY_ALIAS,
		name: 'Dictionary Detail Repository',
		api: () => import('./dictionary-detail.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_DICTIONARY_DETAIL_STORE_ALIAS,
		name: 'Dictionary Detail Store',
		api: () => import('./dictionary-detail.store.js'),
	},
];
