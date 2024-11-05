import { UMB_MEDIA_DETAIL_REPOSITORY_ALIAS, UMB_MEDIA_DETAIL_STORE_ALIAS } from './constants.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEDIA_DETAIL_REPOSITORY_ALIAS,
		name: 'Media Detail Repository',
		api: () => import('./media-detail.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_MEDIA_DETAIL_STORE_ALIAS,
		name: 'Media Detail Store',
		api: () => import('./media-detail.store.js'),
	},
];
