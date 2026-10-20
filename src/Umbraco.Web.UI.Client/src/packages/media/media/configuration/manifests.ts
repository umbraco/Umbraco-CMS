import { UMB_MEDIA_CONFIGURATION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEDIA_CONFIGURATION_REPOSITORY_ALIAS,
		name: 'Media Configuration Repository',
		api: () => import('./configuration.repository.js'),
	},
];
