import { UMB_DICTIONARY_COLLECTION_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';
import { manifests as collectionActionManifests } from './action/manifests.js';
import { UMB_DICTIONARY_COLLECTION_ALIAS } from './constants.js';
import type { ManifestCollection } from '@umbraco-cms/backoffice/collection';

const collectionManifest: ManifestCollection = {
	type: 'collection',
	kind: 'default',
	alias: UMB_DICTIONARY_COLLECTION_ALIAS,
	element: () => import('./dictionary-collection.element.js'),
	name: 'Dictionary Collection',
	meta: {
		repositoryAlias: UMB_DICTIONARY_COLLECTION_REPOSITORY_ALIAS,
	},
};

export const manifests: Array<UmbExtensionManifest> = [
	collectionManifest,
	...collectionRepositoryManifests,
	...collectionViewManifests,
	...collectionActionManifests,
];
