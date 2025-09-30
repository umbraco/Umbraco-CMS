import { UMB_USER_COLLECTION_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';
import { manifests as collectionActionManifests } from './action/manifests.js';
import { UMB_USER_COLLECTION_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collection',
		alias: UMB_USER_COLLECTION_ALIAS,
		name: 'User Collection',
		api: () => import('./user-collection.context.js'),
		element: () => import('./user-collection.element.js'),
		meta: {
			repositoryAlias: UMB_USER_COLLECTION_REPOSITORY_ALIAS,
		},
	},
	...collectionRepositoryManifests,
	...collectionViewManifests,
	...collectionActionManifests,
];
