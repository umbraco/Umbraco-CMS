import { UMB_MEDIA_COLLECTION_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';
import { UmbMediaCollectionContext } from './media-collection.context.js';
import { UMB_MEDIA_COLLECTION_ALIAS } from './index.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const collectionManifest: ManifestTypes = {
	type: 'collection',
	alias: UMB_MEDIA_COLLECTION_ALIAS,
	name: 'Media Collection',
	api: UmbMediaCollectionContext,
	element: () => import('./media-collection.element.js'),
	meta: {
		repositoryAlias: UMB_MEDIA_COLLECTION_REPOSITORY_ALIAS,
	},
};

export const manifests = [
	collectionManifest,
	...collectionRepositoryManifests,
	...collectionViewManifests,
];
