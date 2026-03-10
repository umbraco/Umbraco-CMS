import { EXAMPLE_COLLECTION_ALIAS } from './constants.js';
import { EXAMPLE_COLLECTION_REPOSITORY_ALIAS } from './repository/constants.js';
import { manifests as cardViewManifests } from './card-view/manifests.js';
import { manifests as refViewManifests } from './ref-view/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as tableViewManifests } from './table-view/manifests.js';
import { manifests as filterManifests } from './filter/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collection',
		kind: 'default',
		alias: EXAMPLE_COLLECTION_ALIAS,
		name: 'Example Collection',
		meta: {
			repositoryAlias: EXAMPLE_COLLECTION_REPOSITORY_ALIAS,
		},
	},
	...cardViewManifests,
	...refViewManifests,
	...repositoryManifests,
	...tableViewManifests,
	...filterManifests,
];
