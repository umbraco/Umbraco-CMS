import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as unregisterManifests } from './unregister/manifests.js';
import { manifests as itemManifests } from './item/manifests.js';
import { manifests as pickerDataSourceManifests } from './picker-data-source/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...collectionManifests,
	...unregisterManifests,
	...itemManifests,
	...pickerDataSourceManifests,
];
