import { UMB_USER_COLLECTION_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';
import { manifests as collectionActionManifests } from './action/manifests.js';
import { UmbUserCollectionContext } from './user-collection.context.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_USER_COLLECTION_ALIAS = 'Umb.Collection.User';
const collectionManifest: ManifestTypes = {
	type: 'collection',
	alias: UMB_USER_COLLECTION_ALIAS,
	name: 'User Collection',
	api: UmbUserCollectionContext,
	element: () => import('./user-collection.element.js'),
	meta: {
		repositoryAlias: UMB_USER_COLLECTION_REPOSITORY_ALIAS,
	},
};

export const manifests = [
	collectionManifest,
	...collectionRepositoryManifests,
	...collectionViewManifests,
	...collectionActionManifests,
];
