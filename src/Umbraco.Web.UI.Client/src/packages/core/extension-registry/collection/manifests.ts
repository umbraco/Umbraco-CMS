import { UMB_EXTENSION_COLLECTION_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_EXTENSION_COLLECTION_ALIAS = 'Umb.Collection.Extension';

const collectionManifest: ManifestTypes = {
	type: 'collection',
	kind: 'default',
	alias: UMB_EXTENSION_COLLECTION_ALIAS,
	name: 'Extension Collection',
	meta: {
		repositoryAlias: UMB_EXTENSION_COLLECTION_REPOSITORY_ALIAS,
	},
};

export const manifests = [collectionManifest, ...collectionRepositoryManifests, ...collectionViewManifests];
