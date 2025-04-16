import { UMB_DATA_TYPE_DETAIL_REPOSITORY_ALIAS, UMB_DATA_TYPE_DETAIL_STORE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DATA_TYPE_DETAIL_REPOSITORY_ALIAS,
		name: 'Data Type Detail Repository',
		api: () => import('./data-type-detail.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_DATA_TYPE_DETAIL_STORE_ALIAS,
		name: 'Data Type Detail Store',
		api: () => import('./data-type-detail.store.js'),
	},
];
