import { manifests as detailManifests } from './detail/manifests.js';
import { manifests as itemManifests } from './item/manifests.js';
import { manifests as queryManifests } from './query/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...detailManifests, ...itemManifests, ...queryManifests];
