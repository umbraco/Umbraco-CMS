import {
	UMB_USER_GROUP_COLLECTION_REPOSITORY_ALIAS,
	manifests as repositoryManifests,
} from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';
import { manifests as collectionActionManifests } from './action/manifests.js';
import { UMB_USER_GROUP_COLLECTION_ALIAS } from './constants.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const collection: ManifestTypes = {
	type: 'collection',
	alias: UMB_USER_GROUP_COLLECTION_ALIAS,
	name: 'User Group Collection',
	api: () => import('./user-group-collection.context.js'),
	element: () => import('./user-group-collection.element.js'),
	meta: {
		repositoryAlias: UMB_USER_GROUP_COLLECTION_REPOSITORY_ALIAS,
	},
};

export const manifests: Array<ManifestTypes> = [
	collection,
	...repositoryManifests,
	...collectionViewManifests,
	...collectionActionManifests,
];
