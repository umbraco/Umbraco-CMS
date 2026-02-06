import { UMB_EXTENSION_ITEM_REPOSITORY_ALIAS, UMB_EXTENSION_ITEM_STORE_ALIAS } from './constants.js';
import { UmbExtensionItemStore } from './item.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_EXTENSION_ITEM_REPOSITORY_ALIAS,
		name: 'Extension Item Repository',
		api: () => import('./item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_EXTENSION_ITEM_STORE_ALIAS,
		name: 'Extension Item Store',
		api: UmbExtensionItemStore,
	},
];
