import { manifests as mediaManifests } from './media/manifests.js';
import { manifests as mediaSectionManifests } from './media-section/manifests.js';
import { manifests as mediaTypesManifests } from './media-types/manifests.js';
import { manifests as imagingManifests } from './imaging/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...mediaSectionManifests,
	...mediaManifests,
	...mediaTypesManifests,
	...imagingManifests,
];
