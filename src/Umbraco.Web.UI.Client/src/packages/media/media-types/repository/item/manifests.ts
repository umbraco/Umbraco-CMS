import { UMB_MEDIA_TYPE_ITEM_REPOSITORY_ALIAS, UMB_MEDIA_TYPE_ITEM_STORE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEDIA_TYPE_ITEM_REPOSITORY_ALIAS,
		name: 'Media Type Item Repository',
		api: () => import('./media-type-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_MEDIA_TYPE_ITEM_STORE_ALIAS,
		name: 'Media Type Item Store',
		api: () => import('./media-type-item.store.js'),
	},
];
