import { UMB_EXPORT_MEDIA_TYPE_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_EXPORT_MEDIA_TYPE_REPOSITORY_ALIAS,
		name: 'Export Media Type Repository',
		api: () => import('./media-type-export.repository.js'),
	},
];
