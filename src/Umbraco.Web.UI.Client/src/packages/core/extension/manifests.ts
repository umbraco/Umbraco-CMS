import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as unregisterManifests } from './unregister/manifests.js';
import { manifests as itemManifests } from './item/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...collectionManifests,
	...unregisterManifests,
	...itemManifests,
];
