import { UMB_EXTENSION_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_EXTENSION_COLLECTION_REPOSITORY_ALIAS,
		name: 'Extension Collection Repository',
		api: () => import('./collection.repository.js'),
	},
];
