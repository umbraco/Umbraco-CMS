import { UMB_SEARCH_DETAIL_REPOSITORY_ALIAS, UMB_SEARCH_DETAIL_STORE_ALIAS } from '../constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		name: 'Umbraco Search Detail Repository',
		alias: UMB_SEARCH_DETAIL_REPOSITORY_ALIAS,
		api: () => import('./search-detail.repository.js'),
	},
	{
		type: 'store',
		name: 'Umbraco Search Detail Store',
		alias: UMB_SEARCH_DETAIL_STORE_ALIAS,
		api: () => import('./search-detail.store.js'),
	},
];
