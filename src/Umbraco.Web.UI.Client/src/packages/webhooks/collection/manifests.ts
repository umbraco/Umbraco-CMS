import {
	manifests as collectionRepositoryManifests,
	UMB_WEBHOOK_COLLECTION_REPOSITORY_ALIAS,
} from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';
import { manifests as collectionActionManifests } from './action/manifests.js';
import type { ManifestCollection } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_WEBHOOK_COLLECTION_ALIAS = 'Umb.Collection.Webhooks';

const collectionManifest: ManifestCollection = {
	type: 'collection',
	kind: 'default',
	alias: UMB_WEBHOOK_COLLECTION_ALIAS,
	name: 'Webhook Collection',
	meta: {
		repositoryAlias: UMB_WEBHOOK_COLLECTION_REPOSITORY_ALIAS,
	},
};

export const manifests = [
	collectionManifest,
	...collectionRepositoryManifests,
	...collectionViewManifests,
	...collectionActionManifests,
];
