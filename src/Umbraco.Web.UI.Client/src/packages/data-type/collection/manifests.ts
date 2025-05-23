import { UMB_DATA_TYPE_COLLECTION_REPOSITORY_ALIAS } from './repository/constants.js';
import { UMB_DATA_TYPE_COLLECTION_ALIAS } from './constants.js';
import { manifests as collectionRepositoryManifests } from './repository/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collection',
		kind: 'default',
		alias: UMB_DATA_TYPE_COLLECTION_ALIAS,
		name: 'Data Type Collection',
		meta: {
			repositoryAlias: UMB_DATA_TYPE_COLLECTION_REPOSITORY_ALIAS,
		},
	},
	...collectionRepositoryManifests,
];
