import { EXAMPLE_PRODUCT_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: EXAMPLE_PRODUCT_COLLECTION_REPOSITORY_ALIAS,
		name: 'Product Collection Repository',
		api: () => import('./collection.repository.js'),
	},
];
