import { EXAMPLE_PRODUCT_COLLECTION_ALIAS } from './constants.js';
import { EXAMPLE_PRODUCT_COLLECTION_REPOSITORY_ALIAS } from './repository/constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collection',
		kind: 'default',
		alias: EXAMPLE_PRODUCT_COLLECTION_ALIAS,
		name: 'Product Collection',
		api: () => import('./collection.context.js'),
		meta: {
			repositoryAlias: EXAMPLE_PRODUCT_COLLECTION_REPOSITORY_ALIAS,
		},
	},
	...repositoryManifests,
];
