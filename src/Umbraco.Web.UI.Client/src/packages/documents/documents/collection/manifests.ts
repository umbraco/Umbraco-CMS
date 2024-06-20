import { UMB_DOCUMENT_COLLECTION_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as collectionActionManifests } from './action/manifests.js';
import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';
import { UMB_DOCUMENT_COLLECTION_ALIAS } from './index.js';
import type { ManifestCollection, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const collectionManifest: ManifestCollection = {
	type: 'collection',
	alias: UMB_DOCUMENT_COLLECTION_ALIAS,
	name: 'Document Collection',
	api: () => import('./document-collection.context.js'),
	element: () => import('./document-collection.element.js'),
	meta: {
		repositoryAlias: UMB_DOCUMENT_COLLECTION_REPOSITORY_ALIAS,
	},
};

export const manifests: Array<ManifestTypes> = [
	collectionManifest,
	...collectionActionManifests,
	...collectionRepositoryManifests,
	...collectionViewManifests,
];
