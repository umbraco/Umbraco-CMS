import { UMB_BULK_TRASH_DOCUMENT_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_BULK_TRASH_DOCUMENT_REPOSITORY_ALIAS,
		name: 'Bulk Trash Document Repository',
		api: () => import('./trash.repository.js'),
	},
];
