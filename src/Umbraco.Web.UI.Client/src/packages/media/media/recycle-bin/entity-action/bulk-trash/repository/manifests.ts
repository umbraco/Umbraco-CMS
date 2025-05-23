import { UMB_BULK_TRASH_MEDIA_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_BULK_TRASH_MEDIA_REPOSITORY_ALIAS,
		name: 'Bulk Trash Media Repository',
		api: () => import('./trash.repository.js'),
	},
];
