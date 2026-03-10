import { UMB_PROPERTY_EDITOR_DATA_SOURCE_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_PROPERTY_EDITOR_DATA_SOURCE_COLLECTION_REPOSITORY_ALIAS,
		name: 'Property Editor Data Source Collection Repository',
		api: () => import('./collection.repository.js'),
	},
];
