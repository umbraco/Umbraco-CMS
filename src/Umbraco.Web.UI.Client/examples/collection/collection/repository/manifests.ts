import { EXAMPLE_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: EXAMPLE_COLLECTION_REPOSITORY_ALIAS,
		name: 'Example Collection Repository',
		api: () => import('./collection.repository.js'),
	},
];
