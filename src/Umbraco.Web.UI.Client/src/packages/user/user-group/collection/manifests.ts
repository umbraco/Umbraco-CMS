import { manifests as collectionActionManifests } from './action/manifests.js';
import { UMB_USER_GROUP_COLLECTION_ALIAS } from './constants.js';
import { manifests as textFilterManifests } from './filter/manifests.js';
import { UMB_USER_GROUP_COLLECTION_REPOSITORY_ALIAS } from './repository/constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collection',
		kind: 'default',
		alias: UMB_USER_GROUP_COLLECTION_ALIAS,
		name: 'User Group Collection',
		meta: {
			repositoryAlias: UMB_USER_GROUP_COLLECTION_REPOSITORY_ALIAS,
		},
	},
	...repositoryManifests,
	...collectionViewManifests,
	...collectionActionManifests,
	...textFilterManifests,
];
