import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';
import { manifests as collectionActionManifests } from './action/manifests.js';
import { UMB_MEMBER_GROUP_COLLECTION_ALIAS, UMB_MEMBER_GROUP_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collection',
		kind: 'default',
		alias: UMB_MEMBER_GROUP_COLLECTION_ALIAS,
		name: 'Member Group Collection',
		meta: {
			repositoryAlias: UMB_MEMBER_GROUP_COLLECTION_REPOSITORY_ALIAS,
		},
	},
	...collectionRepositoryManifests,
	...collectionViewManifests,
	...collectionActionManifests,
];
