import {
	manifests as collectionRepositoryManifests,
	UMB_WEBHOOK_COLLECTION_REPOSITORY_ALIAS,
} from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';
import { manifests as collectionActionManifests } from './action/manifests.js';

export const UMB_WEBHOOK_COLLECTION_ALIAS = 'Umb.Collection.Webhook';

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
