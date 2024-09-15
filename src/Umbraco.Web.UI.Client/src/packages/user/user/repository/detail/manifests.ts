import { UMB_USER_DETAIL_REPOSITORY_ALIAS, UMB_USER_DETAIL_STORE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_USER_DETAIL_REPOSITORY_ALIAS,
		name: 'User Detail Repository',
		api: () => import('./user-detail.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_USER_DETAIL_STORE_ALIAS,
		name: 'User Detail Store',
		api: () => import('./user-detail.store.js'),
	},
];
