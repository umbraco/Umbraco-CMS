import { UMB_MEMBER_GROUP_COLLECTION_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';
import { manifests as collectionActionManifests } from './action/manifests.js';

export const UMB_MEMBER_GROUP_COLLECTION_ALIAS = 'Umb.Collection.MemberGroup';

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
