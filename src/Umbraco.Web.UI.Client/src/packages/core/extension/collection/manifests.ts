import { UMB_EXTENSION_COLLECTION_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';
import { manifests as extensionTypeManifests } from './filter/manifests.js';

export const UMB_EXTENSION_COLLECTION_ALIAS = 'Umb.Collection.Extension';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collection',
		alias: UMB_EXTENSION_COLLECTION_ALIAS,
		name: 'Extension Collection',
		api: () => import('./extension-collection.context.js'),
		element: () => import('./extension-collection.element.js'),
		meta: {
			repositoryAlias: UMB_EXTENSION_COLLECTION_REPOSITORY_ALIAS,
		},
	},
	...collectionRepositoryManifests,
	...collectionViewManifests,
	...extensionTypeManifests,
];
