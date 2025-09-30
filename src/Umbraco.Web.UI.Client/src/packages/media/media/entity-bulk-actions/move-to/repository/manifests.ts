import { UMB_BULK_MOVE_MEDIA_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_BULK_MOVE_MEDIA_REPOSITORY_ALIAS,
		name: 'Bulk Move Media Repository',
		api: () => import('./move-to.repository.js'),
	},
];
