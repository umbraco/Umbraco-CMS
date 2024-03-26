import { UMB_MEMBER_COLLECTION_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';
import { manifests as collectionActionManifests } from './action/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEMBER_COLLECTION_ALIAS = 'Umb.Collection.Member';

const collectionManifest: ManifestTypes = {
	type: 'collection',
	alias: UMB_MEMBER_COLLECTION_ALIAS,
	name: 'Member Collection',
	api: () => import('./member-collection.context.js'),
	element: () => import('./member-collection.element.js'),
	meta: {
		repositoryAlias: UMB_MEMBER_COLLECTION_REPOSITORY_ALIAS,
	},
};

export const manifests = [
	collectionManifest,
	...collectionRepositoryManifests,
	...collectionViewManifests,
	...collectionActionManifests,
];
