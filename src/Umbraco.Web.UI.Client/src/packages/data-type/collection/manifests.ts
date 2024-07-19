import { UMB_DATA_TYPE_COLLECTION_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DATA_TYPE_COLLECTION_ALIAS = 'Umb.Collection.DataType';

const collectionManifest: ManifestTypes = {
	type: 'collection',
	kind: 'default',
	alias: UMB_DATA_TYPE_COLLECTION_ALIAS,
	name: 'Data Type Collection',
	meta: {
		repositoryAlias: UMB_DATA_TYPE_COLLECTION_REPOSITORY_ALIAS,
	},
};

export const manifests: Array<ManifestTypes> = [collectionManifest, ...collectionRepositoryManifests];
