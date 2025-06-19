import { UMB_EXTENSION_COLLECTION_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';

export const UMB_EXTENSION_COLLECTION_ALIAS = 'Umb.Collection.Extension';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collection',
		kind: 'default',
		alias: UMB_EXTENSION_COLLECTION_ALIAS,
		name: 'Extension Collection',
		element: () => import('./extension-collection.element.js'),
		meta: {
			repositoryAlias: UMB_EXTENSION_COLLECTION_REPOSITORY_ALIAS,
		},
	},
	...collectionRepositoryManifests,
	...collectionViewManifests,
];
