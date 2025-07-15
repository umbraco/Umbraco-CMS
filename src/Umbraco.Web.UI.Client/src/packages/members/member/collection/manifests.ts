import { UMB_MEMBER_COLLECTION_REPOSITORY_ALIAS } from './repository/constants.js';
import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';
import { manifests as collectionActionManifests } from './action/manifests.js';

export const UMB_MEMBER_COLLECTION_ALIAS = 'Umb.Collection.Member';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collection',
		alias: UMB_MEMBER_COLLECTION_ALIAS,
		name: 'Member Collection',
		api: () => import('./member-collection.context.js'),
		element: () => import('./member-collection.element.js'),
		meta: {
			repositoryAlias: UMB_MEMBER_COLLECTION_REPOSITORY_ALIAS,
		},
	},
	...collectionRepositoryManifests,
	...collectionViewManifests,
	...collectionActionManifests,
];
