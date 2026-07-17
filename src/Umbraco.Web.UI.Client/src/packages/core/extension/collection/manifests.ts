import { UMB_EXTENSION_COLLECTION_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';
import UmbExtensionCollectionElement from './extension-collection.element.js';
import { UMB_EXTENSION_COLLECTION_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collection',
		kind: 'default',
		alias: UMB_EXTENSION_COLLECTION_ALIAS,
		name: 'Extension Collection',
		element: UmbExtensionCollectionElement,
		meta: {
			repositoryAlias: UMB_EXTENSION_COLLECTION_REPOSITORY_ALIAS,
		},
	},
	...collectionRepositoryManifests,
	...collectionViewManifests,
];
