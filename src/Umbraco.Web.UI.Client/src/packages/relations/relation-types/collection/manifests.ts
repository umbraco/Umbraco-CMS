import { UMB_RELATION_TYPE_COLLECTION_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';
import type { ManifestCollection, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_RELATION_TYPE_COLLECTION_ALIAS = 'Umb.Collection.RelationType';

const collectionManifest: ManifestCollection = {
	type: 'collection',
	kind: 'default',
	alias: UMB_RELATION_TYPE_COLLECTION_ALIAS,
	name: 'Relation Type Collection',
	element: () => import('./relation-type-collection.element.js'),
	meta: {
		repositoryAlias: UMB_RELATION_TYPE_COLLECTION_REPOSITORY_ALIAS,
	},
};

export const manifests: Array<ManifestTypes> = [
	collectionManifest,
	...collectionRepositoryManifests,
	...collectionViewManifests,
];
