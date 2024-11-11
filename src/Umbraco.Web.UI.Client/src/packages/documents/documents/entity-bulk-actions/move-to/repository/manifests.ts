import { UMB_BULK_MOVE_DOCUMENT_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_BULK_MOVE_DOCUMENT_REPOSITORY_ALIAS,
		name: 'Bulk Move Document Repository',
		api: () => import('./move-to.repository.js'),
	},
];
