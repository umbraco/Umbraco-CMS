import { manifests as detailManifests } from './detail/manifests.js';
import { manifests as eventManifests } from './event/manifests.js';
import { manifests as itemManifests } from './item/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...detailManifests, ...eventManifests, ...itemManifests];
