import { UMB_MEDIA_ITEM_REPOSITORY_ALIAS, UMB_MEDIA_STORE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEDIA_ITEM_REPOSITORY_ALIAS,
		name: 'Media Item Repository',
		api: () => import('./media-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_MEDIA_STORE_ALIAS,
		name: 'Media Item Store',
		api: () => import('./media-item.store.js'),
	},
];
