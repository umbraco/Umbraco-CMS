import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';
import { manifests as collectionActionManifests } from './action/manifests.js';
import { UMB_WEBHOOK_COLLECTION_ALIAS, UMB_WEBHOOK_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collection',
		kind: 'default',
		alias: UMB_WEBHOOK_COLLECTION_ALIAS,
		name: 'Webhook Collection',
		meta: {
			repositoryAlias: UMB_WEBHOOK_COLLECTION_REPOSITORY_ALIAS,
		},
	},
	...collectionRepositoryManifests,
	...collectionViewManifests,
	...collectionActionManifests,
];
