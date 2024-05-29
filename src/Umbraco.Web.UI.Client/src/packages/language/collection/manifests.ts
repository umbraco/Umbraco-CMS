import { UMB_LANGUAGE_COLLECTION_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';
import { manifests as collectionActionManifests } from './action/manifests.js';
import { UMB_LANGUAGE_COLLECTION_ALIAS } from './constants.js';
import type { ManifestCollection, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const collectionManifest: ManifestCollection = {
	type: 'collection',
	kind: 'default',
	alias: UMB_LANGUAGE_COLLECTION_ALIAS,
	name: 'Language Collection',
	meta: {
		repositoryAlias: UMB_LANGUAGE_COLLECTION_REPOSITORY_ALIAS,
	},
};

export const manifests: Array<ManifestTypes> = [
	collectionManifest,
	...collectionRepositoryManifests,
	...collectionViewManifests,
	...collectionActionManifests,
];
