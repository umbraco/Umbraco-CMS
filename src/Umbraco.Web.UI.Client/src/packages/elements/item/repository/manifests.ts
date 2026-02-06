import { UMB_ELEMENT_ITEM_REPOSITORY_ALIAS, UMB_ELEMENT_STORE_ALIAS } from './constants.js';
import { UmbElementItemStore } from './element-item.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_ELEMENT_ITEM_REPOSITORY_ALIAS,
		name: 'Element Item Repository',
		api: () => import('./element-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_ELEMENT_STORE_ALIAS,
		name: 'Element Item Store',
		api: UmbElementItemStore,
	},
];
