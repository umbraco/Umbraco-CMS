import { UMB_MEDIA_COLLECTION_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as collectionActionManifests } from './action/manifests.js';
import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';
import { UMB_MEDIA_COLLECTION_ALIAS } from './index.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const collectionManifest: ManifestTypes = {
	type: 'collection',
	alias: UMB_MEDIA_COLLECTION_ALIAS,
	name: 'Media Collection',
	api: () => import('./media-collection.context.js'),
	element: () => import('./media-collection.element.js'),
	meta: {
		repositoryAlias: UMB_MEDIA_COLLECTION_REPOSITORY_ALIAS,
	},
};

export const manifests: Array<ManifestTypes> = [
	collectionManifest,
	...collectionActionManifests,
	...collectionRepositoryManifests,
	...collectionViewManifests,
];
