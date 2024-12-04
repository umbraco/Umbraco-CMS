import { UMB_DATA_TYPE_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DATA_TYPE_COLLECTION_REPOSITORY_ALIAS,
		name: 'Data Type Collection Repository',
		api: () => import('./data-type-collection.repository.js'),
	},
];
