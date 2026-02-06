import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as entityActionManifests } from './entity-actions/manifests.js';
import { manifests as itemManifests } from './item/manifests.js';
import { manifests as pickerDataSourceManifests } from './picker-data-source/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...collectionManifests,
	...entityActionManifests,
	...itemManifests,
	...pickerDataSourceManifests,
];
