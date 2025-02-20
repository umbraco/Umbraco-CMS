import { UMB_BULK_DUPLICATE_DOCUMENT_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_BULK_DUPLICATE_DOCUMENT_REPOSITORY_ALIAS,
		name: 'Bulk Duplicate Media Repository',
		api: () => import('./duplicate-to.repository.js'),
	},
];
