import { UMB_MEDIA_TYPE_STRUCTURE_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEDIA_TYPE_STRUCTURE_REPOSITORY_ALIAS,
		name: 'Media Type Structure Repository',
		api: () => import('./media-type-structure.repository.js'),
	},
];
