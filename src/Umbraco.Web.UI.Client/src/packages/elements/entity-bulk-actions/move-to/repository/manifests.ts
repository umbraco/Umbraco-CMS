import { UMB_BULK_MOVE_ELEMENT_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_BULK_MOVE_ELEMENT_REPOSITORY_ALIAS,
		name: 'Bulk Move Element Repository',
		api: () => import('./move-to.repository.js'),
	},
];
