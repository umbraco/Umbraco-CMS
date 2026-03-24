import { DYNAMIC_FACET_COLLECTION_ALIAS } from './constants.js';
import { DYNAMIC_FACET_COLLECTION_REPOSITORY_ALIAS } from './repository/constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collection',
		kind: 'default',
		alias: DYNAMIC_FACET_COLLECTION_ALIAS,
		name: 'Dynamic Facet Filter Collection',
		meta: {
			repositoryAlias: DYNAMIC_FACET_COLLECTION_REPOSITORY_ALIAS,
		},
	},
	...repositoryManifests,
];
