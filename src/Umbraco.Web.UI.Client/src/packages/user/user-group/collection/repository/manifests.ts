import { UMB_USER_GROUP_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_USER_GROUP_COLLECTION_REPOSITORY_ALIAS,
		name: 'User Group Collection Repository',
		api: () => import('./user-group-collection.repository.js'),
	},
];
