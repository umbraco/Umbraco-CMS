import { UMB_MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS, UMB_MEDIA_TYPE_DETAIL_STORE_ALIAS } from './constants.js';
import { UmbMediaTypeDetailStore } from './media-type-detail.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS,
		name: 'Media Types Repository',
		api: () => import('./media-type-detail.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_MEDIA_TYPE_DETAIL_STORE_ALIAS,
		name: 'Media Type Store',
		api: UmbMediaTypeDetailStore,
	},
];
