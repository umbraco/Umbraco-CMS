import { UMB_CLIPBOARD_COLLECTION_ALIAS } from './constants.js';
import { UMB_CLIPBOARD_COLLECTION_REPOSITORY_ALIAS } from './repository/index.js';
import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collection',
		kind: 'default',
		alias: UMB_CLIPBOARD_COLLECTION_ALIAS,
		name: 'Clipboard Collection',
		meta: {
			repositoryAlias: UMB_CLIPBOARD_COLLECTION_REPOSITORY_ALIAS,
		},
	},
	...collectionRepositoryManifests,
	...collectionViewManifests,
];
