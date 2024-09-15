import { UMB_LANGUAGE_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_LANGUAGE_COLLECTION_REPOSITORY_ALIAS,
		name: 'Language Collection Repository',
		api: () => import('./language-collection.repository.js'),
	},
];
