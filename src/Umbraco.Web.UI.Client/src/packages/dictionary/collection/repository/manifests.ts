import { UMB_DICTIONARY_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DICTIONARY_COLLECTION_REPOSITORY_ALIAS,
		name: 'Dictionary Collection Repository',
		api: () => import('./dictionary-collection.repository.js'),
	},
];
