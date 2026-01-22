import { UMB_USER_ITEM_REPOSITORY_ALIAS, UMB_USER_ITEM_STORE_ALIAS } from './constants.js';
import { UmbUserItemStore } from './user-item.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_USER_ITEM_REPOSITORY_ALIAS,
		name: 'User Item Repository',
		api: () => import('./user-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_USER_ITEM_STORE_ALIAS,
		name: 'User Item Store',
		api: UmbUserItemStore,
	},
];
