import { UMB_DUPLICATE_MEDIA_TYPE_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DUPLICATE_MEDIA_TYPE_REPOSITORY_ALIAS,
		name: 'Duplicate Media Type Repository',
		api: () => import('./media-type-duplicate.repository.js'),
	},
];
