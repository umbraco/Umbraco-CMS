import { UMB_USER_GROUP_DETAIL_REPOSITORY_ALIAS, UMB_USER_GROUP_DETAIL_STORE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_USER_GROUP_DETAIL_REPOSITORY_ALIAS,
		name: 'User Group Detail Repository',
		api: () => import('./user-group-detail.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_USER_GROUP_DETAIL_STORE_ALIAS,
		name: 'User Group Detail Store',
		api: () => import('./user-group-detail.store.js'),
	},
];
