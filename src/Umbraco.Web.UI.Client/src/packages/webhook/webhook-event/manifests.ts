import { manifests as collectionManifests } from './collection/repository/manifests.js';
import { manifests as itemManifests } from './item/manifests.js';
import { manifests as modalManifests } from './modal/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...collectionManifests,
	...itemManifests,
	...modalManifests,
	...repositoryManifests,
];
