import { UMB_MEMBER_GROUP_ITEM_REPOSITORY_ALIAS, UMB_MEMBER_GROUP_STORE_ALIAS } from './constants.js';
import { UmbMemberGroupItemStore } from './member-group-item.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEMBER_GROUP_ITEM_REPOSITORY_ALIAS,
		name: 'Member Group Item Repository',
		api: () => import('./member-group-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_MEMBER_GROUP_STORE_ALIAS,
		name: 'Member Group Item Store',
		api: UmbMemberGroupItemStore,
	},
];
