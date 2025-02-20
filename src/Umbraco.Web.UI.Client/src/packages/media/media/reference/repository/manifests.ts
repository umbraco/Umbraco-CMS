import { UMB_MEDIA_REFERENCE_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEDIA_REFERENCE_REPOSITORY_ALIAS,
		name: 'Media Reference Repository',
		api: () => import('./media-reference.repository.js'),
	},
];
