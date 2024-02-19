import { UMB_MEMBER_GROUP_COLLECTION_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';
import { manifests as collectionActionManifests } from './action/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEMBER_GROUP_COLLECTION_ALIAS = 'Umb.Collection.MemberGroup';

const collectionManifest: ManifestTypes = {
	type: 'collection',
	kind: 'default',
	alias: UMB_MEMBER_GROUP_COLLECTION_ALIAS,
	name: 'Member Group Collection',
	meta: {
		repositoryAlias: UMB_MEMBER_GROUP_COLLECTION_REPOSITORY_ALIAS,
	},
};

export const manifests = [
	collectionManifest,
	...collectionRepositoryManifests,
	...collectionViewManifests,
	...collectionActionManifests,
];
