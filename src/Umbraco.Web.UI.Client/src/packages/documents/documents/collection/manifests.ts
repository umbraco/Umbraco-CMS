import { UMB_DOCUMENT_COLLECTION_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_COLLECTION_ALIAS = 'Umb.Collection.Document';

const collectionManifest: ManifestTypes = {
	type: 'collection',
	kind: 'default',
	alias: UMB_DOCUMENT_COLLECTION_ALIAS,
	name: 'Document Collection',
	meta: {
		repositoryAlias: UMB_DOCUMENT_COLLECTION_REPOSITORY_ALIAS,
	},
};

export const manifests = [
	collectionManifest,
	...collectionRepositoryManifests,
];
