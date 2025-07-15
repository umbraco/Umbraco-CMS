import { manifests as detailManifests } from './detail/manifests.js';
import { manifests as itemManifests } from './item/manifests.js';
import { manifests as importManifests } from './import/manifests.js';
import { manifests as exportManifests } from './export/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...detailManifests,
	...itemManifests,
	...importManifests,
	...exportManifests,
];
