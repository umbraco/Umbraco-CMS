import { UMB_USER_COLLECTION_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';
import { manifests as collectionActionManifests } from './action/manifests.js';
import { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_USER_COLLECTION_ALIAS = 'Umb.Collection.User';
const collectionManifest: ManifestTypes = {
	type: 'collection',
	alias: UMB_USER_COLLECTION_ALIAS,
	name: 'User Collection',
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
