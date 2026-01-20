import { manifests as detailManifests } from './detail/manifests.js';
import { manifests as itemManifests } from './item/manifests.js';
import { manifests as compositionManifests } from './composition/manifests.js';
import { manifests as templateManifests } from './template/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...detailManifests,
	...itemManifests,
	...compositionManifests,
	...templateManifests,
];
