import {
	UMB_WEBHOOK_DELIVERY_COLLECTION_ALIAS,
	UMB_WEBHOOK_DELIVERY_COLLECTION_REPOSITORY_ALIAS,
} from './constants.js';
import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collection',
		kind: 'default',
		alias: UMB_WEBHOOK_DELIVERY_COLLECTION_ALIAS,
		name: 'Webhook Delivery Collection',
		meta: {
			repositoryAlias: UMB_WEBHOOK_DELIVERY_COLLECTION_REPOSITORY_ALIAS,
		},
	},
	...collectionRepositoryManifests,
	...collectionViewManifests,
];
