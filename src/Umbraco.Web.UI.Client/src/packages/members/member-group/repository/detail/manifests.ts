import { UMB_MEMBER_GROUP_DETAIL_REPOSITORY_ALIAS, UMB_MEMBER_GROUP_DETAIL_STORE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEMBER_GROUP_DETAIL_REPOSITORY_ALIAS,
		name: 'Member Group Detail Repository',
		api: () => import('./member-group-detail.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_MEMBER_GROUP_DETAIL_STORE_ALIAS,
		name: 'Member Group Detail Store',
		api: () => import('./member-group-detail.store.js'),
	},
];
