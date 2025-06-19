import { UMB_MEDIA_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEDIA_COLLECTION_REPOSITORY_ALIAS,
		name: 'Media Collection Repository',
		api: () => import('./media-collection.repository.js'),
	},
];
