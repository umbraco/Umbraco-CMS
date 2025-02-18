import { UMB_RELATION_TYPE_COLLECTION_ALIAS, UMB_RELATION_TYPE_COLLECTION_REPOSITORY_ALIAS } from './constants.js';
import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collection',
		kind: 'default',
		alias: UMB_RELATION_TYPE_COLLECTION_ALIAS,
		name: 'Relation Type Collection',
		element: () => import('./relation-type-collection.element.js'),
		meta: {
			repositoryAlias: UMB_RELATION_TYPE_COLLECTION_REPOSITORY_ALIAS,
		},
	},
	...collectionRepositoryManifests,
	...collectionViewManifests,
];
