import { UMB_MEDIA_TYPE_IMPORT_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEDIA_TYPE_IMPORT_REPOSITORY_ALIAS,
		name: 'Import Media Type Repository',
		api: () => import('./media-type-import.repository.js'),
	},
];
