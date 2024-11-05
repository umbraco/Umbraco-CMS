import { UMB_USER_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_USER_COLLECTION_REPOSITORY_ALIAS,
		name: 'User Collection Repository',
		api: () => import('./user-collection.repository.js'),
	},
];
