import { manifests as collectionActionManifests } from './action/manifests.js';
import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';
import { UMB_MEDIA_COLLECTION_REPOSITORY_ALIAS, UMB_MEDIA_COLLECTION_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collection',
		alias: UMB_MEDIA_COLLECTION_ALIAS,
		name: 'Media Collection',
		api: () => import('./media-collection.context.js'),
		element: () => import('./media-collection.element.js'),
		meta: {
			repositoryAlias: UMB_MEDIA_COLLECTION_REPOSITORY_ALIAS,
		},
	},
	...collectionActionManifests,
	...collectionRepositoryManifests,
	...collectionViewManifests,
];
