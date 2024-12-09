import { UMB_MEDIA_VALIDATION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEDIA_VALIDATION_REPOSITORY_ALIAS,
		name: 'Media Validation Repository',
		api: () => import('./media-validation.repository.js'),
	},
];
