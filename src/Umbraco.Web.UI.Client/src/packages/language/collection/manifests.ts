import { UMB_LANGUAGE_COLLECTION_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';
import { manifests as collectionActionManifests } from './action/manifests.js';
import type { ManifestCollection } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_LANGUAGE_COLLECTION_ALIAS = 'Umb.Collection.Language';

const collectionManifest: ManifestCollection = {
	type: 'collection',
	kind: 'default',
	alias: UMB_LANGUAGE_COLLECTION_ALIAS,
	name: 'Language Collection',
	meta: {
		repositoryAlias: UMB_LANGUAGE_COLLECTION_REPOSITORY_ALIAS,
	},
};

export const manifests = [
	collectionManifest,
	...collectionRepositoryManifests,
	...collectionViewManifests,
	...collectionActionManifests,
];
