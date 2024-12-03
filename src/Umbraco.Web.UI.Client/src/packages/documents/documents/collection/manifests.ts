import { manifests as collectionActionManifests } from './action/manifests.js';
import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';
import { UMB_DOCUMENT_COLLECTION_REPOSITORY_ALIAS, UMB_DOCUMENT_COLLECTION_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collection',
		alias: UMB_DOCUMENT_COLLECTION_ALIAS,
		name: 'Document Collection',
		api: () => import('./document-collection.context.js'),
		element: () => import('./document-collection.element.js'),
		meta: {
			repositoryAlias: UMB_DOCUMENT_COLLECTION_REPOSITORY_ALIAS,
		},
	},
	...collectionActionManifests,
	...collectionRepositoryManifests,
	...collectionViewManifests,
];
