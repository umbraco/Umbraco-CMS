import { EXAMPLE_PRODUCT_COLLECTION_ALIAS } from './constants.js';
import { EXAMPLE_PRODUCT_COLLECTION_REPOSITORY_ALIAS } from './repository/constants.js';
import { manifests as filterManifests } from './filter/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as tableViewManifests } from './table-view/manifests.js';

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
	...filterManifests,
	...repositoryManifests,
	...tableViewManifests,
];
